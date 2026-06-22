// <copyright file="BulkheadRejectedException.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace Polly.Contrib.Bulkhead;

/// <summary>
/// Exception thrown when a call is rejected because the bulkhead's concurrency
/// limit and queue are both full.
/// </summary>
public sealed class BulkheadRejectedException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="BulkheadRejectedException"/>.
    /// </summary>
    public BulkheadRejectedException()
        : base("The bulkhead is full and the call was rejected.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BulkheadRejectedException"/> with a custom message.
    /// </summary>
    public BulkheadRejectedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BulkheadRejectedException"/> with a message and inner exception.
    /// </summary>
    public BulkheadRejectedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
