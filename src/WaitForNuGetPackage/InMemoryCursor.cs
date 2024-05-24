// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A class representing an in-memory cursor. This class cannot be inherited.
/// </summary>
internal sealed class InMemoryCursor : ICursor
{
    /// <summary>
    /// Gets or sets the current cursor value.
    /// </summary>
    public DateTimeOffset? Value { get; set; }

    /// <inheritdoc />
    public Task<DateTimeOffset?> GetAsync() => Task.FromResult(Value);

    /// <inheritdoc />
    public Task SetAsync(DateTimeOffset value)
    {
        Value = value;
        return Task.CompletedTask;
    }
}
