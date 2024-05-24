// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using NuGet.Protocol;
using NuGet.Protocol.Catalog;
using NuGet.Protocol.Core.Types;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A class representing the command to wait for a new version of one or
/// more NuGet packages to be published. This class cannot be inherited.
/// </summary>
internal sealed class WaitCommand(
    IAnsiConsole console,
    CatalogProcessor processor,
    PackageWaitContext packages,
    TimeProvider timeProvider,
    CancellationTokenSource cancellationTokenSource) : AsyncCommand<WaitCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, WaitCommandSettings settings)
    {
        if (settings.NoLogo is not true)
        {
            console.MarkupLineInterpolated($"[bold {Color.Purple}]WaitForNuGetPackage[/]");
            console.MarkupLineInterpolated($"[{Color.Blue}]v{Waiter.Version} (.NET v{Environment.Version})[/]");
            console.WriteLine();
        }

        var table = new Table();

        table.AddColumn("[bold]Package ID[/]");
        table.AddColumn(new TableColumn("[bold]Package Version[/]").RightAligned());

        foreach (var package in packages.DesiredPackages.OrderBy((p) => p.Id))
        {
            table.AddRow(package.Id, package.Version);
        }

        console.Write(table);

        var stopwatch = Stopwatch.StartNew();

        await console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("purple"))
            .StartAsync("Waiting for NuGet packages...", async (_) => await WaitForPackagesAsync(settings));

        stopwatch.Stop();

        int result = 0;
        Color color = packages.AllPublished ? Color.Green : Color.Yellow;

        console.WriteLine();

        if (cancellationTokenSource.Token.IsCancellationRequested)
        {
            console.MarkupLineInterpolated($"[{Color.Yellow}]{Emoji.Known.Warning}  Processing cancelled or timed out.[/]");
            console.WriteLine();

            result = 2;
        }

        var elapsed = new TimeSpan(TimeSpan.TicksPerSecond * (stopwatch.Elapsed.Ticks / TimeSpan.TicksPerSecond));
        var count = packages.ObservedPackages.Count;
        var plural = count is 1 ? string.Empty : "s";

        console.MarkupLineInterpolated($"[{color}]{count} package{plural} found published after {elapsed}.[/]");

        return result;
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

    private async Task WaitForPackagesAsync(WaitCommandSettings settings)
    {
        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        DateTimeOffset publishedSince = utcNow.Add(-(settings.Since ?? TimeSpan.Zero));

        if (await AllPackagesArePublishedAsync(settings, publishedSince))
        {
            // All the packages are already published.
            return;
        }

        while (!cancellationTokenSource.Token.IsCancellationRequested && !packages.AllPublished)
        {
            if (!await processor.ProcessAsync())
            {
                break;
            }
        }

        if (packages.AllPublished)
        {
            await Parallel.ForEachAsync(
                packages.ObservedPackages,
                async (package, _) => await WaitForIndexAsync(package.Id, package.Version, settings));
        }
    }

    private async Task WaitForIndexAsync(
        string packageId,
        string packageVersion,
        WaitCommandSettings settings)
    {
        var delay = TimeSpan.FromSeconds(10);
        var repository = Repository.Factory.GetCoreV3(settings.ServiceIndexUrl);
        var resource = await repository.GetResourceAsync<PackageSearchResource>();

        var cancellationToken = cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (await IsPackagePublishedAsync(resource, packageId, packageVersion, null, cancellationToken))
            {
                break;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task<bool> AllPackagesArePublishedAsync(
        WaitCommandSettings settings,
        DateTimeOffset publishedSince)
    {
        var repository = Repository.Factory.GetCoreV3(settings.ServiceIndexUrl);
        var resource = await repository.GetResourceAsync<PackageSearchResource>();

        bool result = true;

        foreach (var package in packages.DesiredPackages)
        {
            if (await IsPackagePublishedAsync(
                    resource,
                    package.Id,
                    package.Version,
                    publishedSince,
                    cancellationTokenSource.Token))
            {
                packages.MarkPublished(package.Id, package.Version);
            }
            else
            {
                result = false;
            }
        }

        return result;
    }
}
