// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A class representing the command to wait for a new version of one or
/// more NuGet packages to be published. This class cannot be inherited.
/// </summary>
internal sealed class WaitCommand(
    IAnsiConsole console,
    PackageWaitContext packages,
    NuGetRepository repository,
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

#if NET10_0_OR_GREATER
        var comparer = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);
#else
        var comparer = StringComparer.OrdinalIgnoreCase;
#endif

        foreach (var package in packages.DesiredPackages.OrderBy((p) => p.Id, comparer).ThenBy((p) => p.Version, comparer))
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

    private async Task WaitForPackagesAsync(WaitCommandSettings settings)
        => await repository.WaitForPackagesAsync(packages, settings, cancellationTokenSource.Token);
}
