// <copyright file="BulkheadResilienceStrategyTests.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

using FluentAssertions;
using Polly.Contrib.Bulkhead;

namespace Polly.Contrib.Bulkhead.Tests;

[TestFixture]
public class BulkheadResilienceStrategyTests
{
    [Test]
    public async Task UnderCapacity_ExecutesSuccessfully()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: 2)
            .Build();

        var result = await pipeline.ExecuteAsync(async _ =>
        {
            await Task.Delay(10);
            return 42;
        });

        result.Should().Be(42);
    }

    [Test]
    public async Task AtCapacity_NoQueue_RejectsImmediately()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: 1, maxQueuedActions: 0)
            .Build();

        var blocker = new TaskCompletionSource();

        // Occupy the single slot
        var occupying = pipeline.ExecuteAsync(async _ => await blocker.Task).AsTask();

        await Task.Delay(20); // let occupying task start

        // Second call should be rejected
        var act = () => pipeline.ExecuteAsync(_ => ValueTask.CompletedTask).AsTask();

        await act.Should().ThrowAsync<BulkheadRejectedException>();

        blocker.SetResult();
        await occupying;
    }

    [Test]
    public async Task WithQueue_SecondCallQueuesAndEventuallyExecutes()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: 1, maxQueuedActions: 1)
            .Build();

        var blocker = new TaskCompletionSource();
        var executionOrder = new List<int>();

        var first = pipeline.ExecuteAsync(async _ =>
        {
            await blocker.Task;
            executionOrder.Add(1);
        }).AsTask();

        await Task.Delay(20); // let first task start

        var second = pipeline.ExecuteAsync(_ =>
        {
            executionOrder.Add(2);
            return ValueTask.CompletedTask;
        }).AsTask();

        blocker.SetResult();
        await Task.WhenAll(first, second);

        executionOrder.Should().ContainInOrder(1, 2);
    }

    [Test]
    public async Task QueueFull_RejectsAdditionalCalls()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: 1, maxQueuedActions: 1)
            .Build();

        var blocker = new TaskCompletionSource();

        var first = pipeline.ExecuteAsync(async _ => await blocker.Task).AsTask();
        await Task.Delay(20);

        // This one should queue
        var second = pipeline.ExecuteAsync(_ => ValueTask.CompletedTask).AsTask();

        // This one should be rejected (queue is full)
        var act = () => pipeline.ExecuteAsync(_ => ValueTask.CompletedTask).AsTask();
        await act.Should().ThrowAsync<BulkheadRejectedException>();

        blocker.SetResult();
        await Task.WhenAll(first, second);
    }

    [Test]
    public async Task OnBulkheadRejected_IsInvokedOnRejection()
    {
        var rejected = false;

        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(new BulkheadStrategyOptions
            {
                MaxConcurrency = 1,
                MaxQueuedActions = 0,
                OnBulkheadRejected = _ =>
                {
                    rejected = true;
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

        var blocker = new TaskCompletionSource();
        var first = pipeline.ExecuteAsync(async _ => await blocker.Task).AsTask();
        await Task.Delay(20);

        try { await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask); } catch { }

        rejected.Should().BeTrue();

        blocker.SetResult();
        await first;
    }

    [Test]
    public async Task ConcurrentExecutions_NeverExceedMaxConcurrency()
    {
        const int maxConcurrency = 3;
        var pipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: maxConcurrency, maxQueuedActions: 10)
            .Build();

        var current = 0;
        var maxObserved = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            pipeline.ExecuteAsync(async _ =>
            {
                var c = Interlocked.Increment(ref current);
                lock (lockObj) maxObserved = Math.Max(maxObserved, c);
                await Task.Delay(30);
                Interlocked.Decrement(ref current);
            }).AsTask()).ToList();

        await Task.WhenAll(tasks);

        maxObserved.Should().BeLessThanOrEqualTo(maxConcurrency);
    }

    [Test]
    public void InvalidMaxConcurrency_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddBulkhead(maxConcurrency: 0)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NullOptions_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddBulkhead((BulkheadStrategyOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
