// <copyright file="BulkheadResilienceStrategy.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace Polly.Contrib.Bulkhead;

/// <summary>
/// A Polly v8 resilience strategy that limits concurrent executions and queued actions.
/// Calls exceeding <see cref="BulkheadStrategyOptions.MaxConcurrency"/> are queued up to
/// <see cref="BulkheadStrategyOptions.MaxQueuedActions"/>; further calls are rejected
/// with <see cref="BulkheadRejectedException"/>.
/// </summary>
internal sealed class BulkheadResilienceStrategy<TResult> : ResilienceStrategy<TResult>, IDisposable
{
    private readonly BulkheadStrategyOptions _options;
    private readonly SemaphoreSlim _concurrencySlot;
    private readonly SemaphoreSlim? _queueSlot;

    internal BulkheadResilienceStrategy(BulkheadStrategyOptions options)
    {
        if (options.MaxConcurrency <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "MaxConcurrency must be greater than zero.");
        if (options.MaxQueuedActions < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "MaxQueuedActions must be zero or greater.");

        _options = options;
        _concurrencySlot = new SemaphoreSlim(options.MaxConcurrency, options.MaxConcurrency);
        _queueSlot = options.MaxQueuedActions > 0
            ? new SemaphoreSlim(options.MaxQueuedActions, options.MaxQueuedActions)
            : null;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Try to acquire a queue slot (only needed when there is a queue)
        bool hasQueueSlot = false;
        if (_queueSlot is not null)
        {
            hasQueueSlot = _queueSlot.Wait(0);
            if (!hasQueueSlot && _concurrencySlot.CurrentCount == 0)
            {
                // Queue full and no free concurrency slot — reject
                await RejectAsync(context).ConfigureAwait(false);
                return Outcome.FromException<TResult>(new BulkheadRejectedException());
            }
        }
        else if (_concurrencySlot.CurrentCount == 0)
        {
            // No queuing — reject immediately
            await RejectAsync(context).ConfigureAwait(false);
            return Outcome.FromException<TResult>(new BulkheadRejectedException());
        }

        try
        {
            // Wait for a concurrency slot (will succeed immediately or after queuing)
            await _concurrencySlot.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (hasQueueSlot)
                _queueSlot!.Release();
        }

        try
        {
            return await callback(context, state).ConfigureAwait(false);
        }
        finally
        {
            _concurrencySlot.Release();
        }
    }

    private async ValueTask RejectAsync(ResilienceContext context)
    {
        if (_options.OnBulkheadRejected is not null)
            await _options.OnBulkheadRejected(new BulkheadRejectedArguments(context)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _concurrencySlot.Dispose();
        _queueSlot?.Dispose();
    }
}
