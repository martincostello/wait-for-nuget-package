// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
using Spectre.Console;

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class CatalogLeafProcessor : ICatalogLeafProcessor
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _cancellationToken;
    private readonly HashSet<DesiredNuGetPackage> _packages;

    public CatalogLeafProcessor(
        IAnsiConsole console,
        WaitCommandSettings settings,
        CancellationTokenSource cancellationTokenSource)
    {
        _console = console;
        _cancellationToken = cancellationTokenSource.Token;
        _packages = [];

        foreach (var package in settings.Packages)
        {
            int index = package.IndexOf('@', StringComparison.Ordinal);

            DesiredNuGetPackage desired;

            if (index > 0)
            {
                desired = new(package[..index], package[(index + 1)..]);
            }
            else
            {
                desired = new(package);
            }

            _packages.Add(desired);
        }
    }

    public Task<bool> ProcessPackageDeleteAsync(PackageDeleteCatalogLeaf leaf)
        => Task.FromResult(!_cancellationToken.IsCancellationRequested);

    public Task<bool> ProcessPackageDetailsAsync(PackageDetailsCatalogLeaf leaf)
    {
        if (_packages.Any((p) => p.Equals(leaf)))
        {
            var timestamp = $"[{leaf.CommitTimestamp:u}]";
            _console.MarkupLineInterpolated($"{timestamp} Package [purple]{leaf.PackageId}@{leaf.PackageVersion}[/] was published.");

            _packages.RemoveWhere((p) => p.Equals(leaf));
        }

        // TODO Need a way to signal to stop processing without making the CatalogProcessor log a warning about unprocessed pages
        return Task.FromResult(!_cancellationToken.IsCancellationRequested);
    }
}
