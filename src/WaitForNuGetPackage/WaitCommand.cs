﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
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

        var cancellationToken = cancellationTokenSource.Token;

        await console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("purple"))
            .StartAsync("Watching NuGet packages...", async (_) =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!await processor.ProcessAsync())
                    {
                        break;
                    }
                }

                // TODO Wait for the package(s) to be indexed
                // https://azuresearch-usnc.nuget.org/query?q=packageid:{NAME}&prerelease=true&semVerLevel=2.0.0
            });

        if (cancellationToken.IsCancellationRequested)
        {
            console.MarkupLineInterpolated($"[{Color.Yellow}]{Emoji.Known.Warning}  Processing cancelled or timed out.[/]");
            return 2;
        }

        return 0;
    }
}
