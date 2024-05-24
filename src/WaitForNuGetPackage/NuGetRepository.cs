// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol;
using NuGet.Protocol.Catalog;
using NuGet.Protocol.Core.Types;

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class NuGetRepository(
    CatalogProcessor processor,
    TimeProvider timeProvider)
{
    public async Task WaitForPackagesAsync(
        PackageWaitContext context,
        WaitCommandSettings settings,
        CancellationToken cancellationToken)
    {
        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        DateTimeOffset publishedSince = utcNow.Add(-(settings.Since ?? TimeSpan.Zero));

        if (await AllPackagesArePublishedAsync(context, settings, publishedSince, cancellationToken))
        {
            // All the packages are already published.
            return;
        }

        while (!cancellationToken.IsCancellationRequested && !context.AllPublished)
        {
            if (!await processor.ProcessAsync())
            {
                break;
            }
        }

        if (context.AllPublished)
        {
            await Parallel.ForEachAsync(
                context.ObservedPackages,
                cancellationToken,
                async (package, token) => await WaitForIndexAsync(package.Id, package.Version, settings, token));
        }
    }

    private static async Task<bool> IsPackagePublishedAsync(
        PackageSearchResource resource,
        string packageId,
        string packageVersion,
        DateTimeOffset? publishedSince,
        CancellationToken cancellationToken)
    {
        var searchTerm = $"packageid:{packageId}";
        var filters = new SearchFilter(includePrerelease: true);
        var skip = 0;
        var take = 1;

        var results = await resource.SearchAsync(
            searchTerm,
            filters,
            skip,
            take,
            NuGet.Common.NullLogger.Instance,
            cancellationToken);

        foreach (var result in results)
        {
            if (string.Equals(result.Identity.Id, packageId, StringComparison.OrdinalIgnoreCase))
            {
                bool correctVersion =
                    string.Equals(result.Identity.Version.ToNormalizedString(), packageVersion, StringComparison.OrdinalIgnoreCase) ||
                    (packageVersion is DesiredNuGetPackage.AnyVersion && result.Published >= publishedSince);

                if (correctVersion && result.IsListed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static async Task WaitForIndexAsync(
        string packageId,
        string packageVersion,
        WaitCommandSettings settings,
        CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(10);
        var repository = Repository.Factory.GetCoreV3(settings.ServiceIndexUrl);
        var resource = await repository.GetResourceAsync<PackageSearchResource>();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (await IsPackagePublishedAsync(resource, packageId, packageVersion, null, cancellationToken))
            {
                break;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    private static async Task<bool> AllPackagesArePublishedAsync(
        PackageWaitContext context,
        WaitCommandSettings settings,
        DateTimeOffset publishedSince,
        CancellationToken cancellationToken)
    {
        var repository = Repository.Factory.GetCoreV3(settings.ServiceIndexUrl);
        var resource = await repository.GetResourceAsync<PackageSearchResource>();

        bool result = true;

        foreach (var package in context.DesiredPackages)
        {
            if (await IsPackagePublishedAsync(
                    resource,
                    package.Id,
                    package.Version,
                    publishedSince,
                    cancellationToken))
            {
                context.MarkPublished(package.Id, package.Version);
            }
            else
            {
                result = false;
            }
        }

        return result;
    }
}
