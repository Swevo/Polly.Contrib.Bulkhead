// <copyright file="BulkheadStrategyOptions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace Polly.Contrib.Bulkhead;

/// <summary>
/// Options for configuring the <see cref="BulkheadResilienceStrategy{TResult}"/>.
/// </summary>
public sealed class BulkheadStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="BulkheadStrategyOptions"/>.
    /// </summary>
    public BulkheadStrategyOptions() => Name = "Bulkhead";

    /// <summary>
    /// The maximum number of concurrent executions permitted.
    /// Must be greater than zero. Defaults to <c>1</c>.
    /// </summary>
    public int MaxConcurrency { get; set; } = 1;

    /// <summary>
    /// The maximum number of calls that may queue while waiting for a slot.
    /// When the queue is also full, calls are rejected with <see cref="BulkheadRejectedException"/>.
    /// Defaults to <c>0</c> (no queuing — reject immediately when at capacity).
    /// </summary>
    public int MaxQueuedActions { get; set; } = 0;

    /// <summary>
    /// An optional callback invoked when a call is rejected by the bulkhead.
    /// </summary>
    public Func<BulkheadRejectedArguments, ValueTask>? OnBulkheadRejected { get; set; }
}
