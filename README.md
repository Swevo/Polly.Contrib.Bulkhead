# Polly.Contrib.Bulkhead

<img src="icon.png" width="100" align="right" />

[![NuGet](https://img.shields.io/nuget/v/Polly.Contrib.Bulkhead.svg)](https://www.nuget.org/packages/Polly.Contrib.Bulkhead)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Polly.Contrib.Bulkhead.svg)](https://www.nuget.org/packages/Polly.Contrib.Bulkhead)
[![CI](https://github.com/Swevo/Polly.Contrib.Bulkhead/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/Polly.Contrib.Bulkhead/actions/workflows/build.yml)

Bulkhead isolation strategy for **Polly v8** resilience pipelines. Limits concurrent executions and queued actions, rejecting excess calls with `BulkheadRejectedException`.

Polly v8 replaced the v7 `BulkheadPolicy` with the more general `RateLimiter` strategy. This package restores the familiar bulkhead semantics — explicit **max concurrency** and **max queue depth** — as a first-class Polly v8 resilience strategy.

## Install

```
dotnet add package Polly.Contrib.Bulkhead
```

## Usage

### Simple concurrency limit

```csharp
using Polly.Contrib.Bulkhead;

var pipeline = new ResiliencePipelineBuilder()
    .AddBulkhead(maxConcurrency: 10)
    .Build();

await pipeline.ExecuteAsync(async ct => await CallDownstreamAsync(ct), cancellationToken);
```

### With a queue

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddBulkhead(maxConcurrency: 10, maxQueuedActions: 20)
    .Build();
```

Calls beyond the 10 concurrent slots queue up. If the queue also fills (>20 waiting), further calls throw `BulkheadRejectedException`.

### Full options

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddBulkhead(new BulkheadStrategyOptions
    {
        MaxConcurrency = 10,
        MaxQueuedActions = 20,
        OnBulkheadRejected = args =>
        {
            logger.LogWarning("Bulkhead rejected call for {Operation}", args.Context.OperationKey);
            return ValueTask.CompletedTask;
        },
    })
    .Build();
```

### Generic pipeline

```csharp
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddBulkhead<HttpResponseMessage>(maxConcurrency: 5)
    .AddRetry(...)
    .Build();
```

## Behaviour

| Scenario | Outcome |
|---|---|
| Slots available | Executes immediately |
| Slots full, queue not full | Waits in queue |
| Slots full, queue full | Throws `BulkheadRejectedException` |
| No queuing (`MaxQueuedActions = 0`) | Rejects immediately when at capacity |

## Composition

Place bulkhead **before** retry so rejected calls don't get retried:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddBulkhead(maxConcurrency: 10)  // 1. limit concurrency first
    .AddRetry(...)                     // 2. retry transient failures
    .AddCircuitBreaker(...)            // 3. protect downstream
    .Build();
```

## Support

If Polly.Contrib.Bulkhead helps protect your services, consider supporting the project:

[![Sponsor](https://img.shields.io/badge/Sponsor-%E2%9D%A4-pink?logo=github)](https://github.com/sponsors/Swevo)

> 💼 **Need .NET resilience help?** Visit [solidqualitysolutions.com](https://solidqualitysolutions.com/) for consulting and architecture services.

## Related packages

| Package | Description |
|---|---|
| [PollyChaos](https://www.nuget.org/packages/PollyChaos) | Chaos engineering — inject faults & latency (Simmy for v8) |
| [PollyMediatR](https://www.nuget.org/packages/PollyMediatR) | Polly v8 pipelines for MediatR request handlers |
| [PollyEFCore](https://www.nuget.org/packages/PollyEFCore) | Polly v8 resilience for EF Core queries and SaveChanges |
| [PollyBackoff](https://www.nuget.org/packages/PollyBackoff) | Backoff delay strategies |
| [PollyHealthChecks](https://www.nuget.org/packages/PollyHealthChecks) | [![Downloads](https://img.shields.io/nuget/dt/PollyHealthChecks.svg)](https://www.nuget.org/packages/PollyHealthChecks) | ASP.NET Core health checks for Polly v8 circuit breakers |
| [PollyOpenAI](https://www.nuget.org/packages/PollyOpenAI) | [![Downloads](https://img.shields.io/nuget/dt/PollyOpenAI.svg)](https://www.nuget.org/packages/PollyOpenAI) | Polly v8 resilience for OpenAI and Azure OpenAI — retry on 429, Retry-After, circuit breaker |
| [PollyRedis](https://www.nuget.org/packages/PollyRedis) | [![Downloads](https://img.shields.io/nuget/dt/PollyRedis.svg)](https://www.nuget.org/packages/PollyRedis) | Polly v8 resilience for StackExchange.Redis — retry, circuit breaker, timeout |
| [PollyCaching](https://www.nuget.org/packages/PollyCaching) | Caching resilience strategy |
| [PollyRateLimiter](https://www.nuget.org/packages/PollyRateLimiter) | Rate limiting strategies |
| [PollyOpenTelemetry](https://www.nuget.org/packages/PollyOpenTelemetry) | OpenTelemetry metrics & tracing |

## License

MIT
