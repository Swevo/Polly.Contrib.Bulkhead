// <copyright file="ResiliencePipelineBuilderExtensions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace Polly.Contrib.Bulkhead;

/// <summary>
/// Extension methods for adding bulkhead isolation to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class ResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a bulkhead isolation strategy using the supplied <see cref="BulkheadStrategyOptions"/>.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="options">Bulkhead strategy configuration.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ResiliencePipelineBuilder AddBulkhead(
        this ResiliencePipelineBuilder builder,
        BulkheadStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        return builder.AddStrategy(_ => new BulkheadResilienceStrategy<object>(options), options);
    }

    /// <summary>
    /// Adds a bulkhead isolation strategy using the supplied <see cref="BulkheadStrategyOptions"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="options">Bulkhead strategy configuration.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ResiliencePipelineBuilder<TResult> AddBulkhead<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        BulkheadStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        return builder.AddStrategy(_ => new BulkheadResilienceStrategy<TResult>(options), options);
    }

    /// <summary>
    /// Adds a bulkhead isolation strategy with the specified concurrency limit.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="maxConcurrency">Maximum number of concurrent executions.</param>
    /// <param name="maxQueuedActions">Maximum number of queued actions. Defaults to <c>0</c>.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ResiliencePipelineBuilder AddBulkhead(
        this ResiliencePipelineBuilder builder,
        int maxConcurrency,
        int maxQueuedActions = 0) =>
        builder.AddBulkhead(new BulkheadStrategyOptions
        {
            MaxConcurrency = maxConcurrency,
            MaxQueuedActions = maxQueuedActions,
        });

    /// <summary>
    /// Adds a bulkhead isolation strategy with the specified concurrency limit.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="maxConcurrency">Maximum number of concurrent executions.</param>
    /// <param name="maxQueuedActions">Maximum number of queued actions. Defaults to <c>0</c>.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ResiliencePipelineBuilder<TResult> AddBulkhead<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        int maxConcurrency,
        int maxQueuedActions = 0) =>
        builder.AddBulkhead(new BulkheadStrategyOptions
        {
            MaxConcurrency = maxConcurrency,
            MaxQueuedActions = maxQueuedActions,
        });
}
