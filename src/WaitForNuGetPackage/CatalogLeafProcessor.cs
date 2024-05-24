// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class CatalogLeafProcessor(
    PackageWaitContext context,
    CancellationTokenSource cancellationTokenSource) : ICatalogLeafProcessor
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public Task<bool> ProcessPackageDeleteAsync(PackageDeleteCatalogLeaf leaf)
        => NotCancelled();

    public Task<bool> ProcessPackageDetailsAsync(PackageDetailsCatalogLeaf leaf)
    {
        _ = context.Process(leaf);
        return NotCancelled();
    }

    private Task<bool> NotCancelled()
        => Task.FromResult(!cancellationTokenSource.IsCancellationRequested);
}
