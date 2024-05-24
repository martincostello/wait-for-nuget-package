// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
using Spectre.Console;

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class CatalogLeafProcessor(
    IAnsiConsole console,
    CancellationTokenSource cancellationTokenSource) : ICatalogLeafProcessor
{
    public Task<bool> ProcessPackageDeleteAsync(PackageDeleteCatalogLeaf leaf)
        => Task.FromResult(!cancellationTokenSource.IsCancellationRequested);

    public Task<bool> ProcessPackageDetailsAsync(PackageDetailsCatalogLeaf leaf)
    {
        var timestamp = $"[{leaf.CommitTimestamp:u}]";
        console.MarkupLineInterpolated($"{timestamp} Package published: [purple]{leaf.PackageId}[/]@{leaf.PackageVersion}");
        return Task.FromResult(!cancellationTokenSource.IsCancellationRequested);
    }
}
