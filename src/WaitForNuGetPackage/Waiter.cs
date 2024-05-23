// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// Waits for a new version of a NuGet package to be published.
/// </summary>
public static class Waiter
{
    /// <summary>
    /// Waits for a new version of one or more NuGet packages to be published as an asynchronous operation.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="args">The arguments passed to the application.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to wait for the package(s) to be published.
    /// </returns>
    public static async Task<int> RunAsync(
        IAnsiConsole console,
        IReadOnlyCollection<string> args,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        console.WriteLine($"Arguments: [ {string.Join(", ", args)} ]");
        return await Task.FromResult(0);
    }
}
