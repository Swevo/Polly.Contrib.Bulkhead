// <copyright file="BulkheadRejectedArguments.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace Polly.Contrib.Bulkhead;

/// <summary>
/// Arguments passed to <see cref="BulkheadStrategyOptions.OnBulkheadRejected"/> when a call is rejected.
/// </summary>
/// <param name="Context">The resilience context for the rejected call.</param>
public readonly record struct BulkheadRejectedArguments(ResilienceContext Context);
