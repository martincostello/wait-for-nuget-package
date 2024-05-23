// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using NuGet.Protocol.Catalog;
using Spectre.Console;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// Waits for a new version of a NuGet package to be published.
/// </summary>
public static class Waiter
{
    //// See https://github.com/NuGet/Samples/blob/ec30a2b7c54c2d09e5a476444a2c7a8f2f289d49/CatalogReaderExample

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
        ArgumentNullException.ThrowIfNull(args);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        using var httpClient = new HttpClient();

        var jsonClient = new SimpleHttpClient(httpClient, loggerFactory.CreateLogger<SimpleHttpClient>());
        var client = new CatalogClient(jsonClient, loggerFactory.CreateLogger<CatalogClient>());
        var leafProcessor = new CatalogLeafProcessor(console, cancellationToken);

        var cursor = new InMemoryCursor();
        var settings = new CatalogProcessorSettings()
        {
            DefaultMinCommitTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
            ExcludeRedundantLeaves = false,
        };

        var processor = new CatalogProcessor(
            cursor,
            client,
            leafProcessor,
            settings,
            loggerFactory.CreateLogger<CatalogProcessor>());

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!await processor.ProcessAsync())
            {
                break;
            }
        }

        return await Task.FromResult(0);
    }

    private sealed class CatalogLeafProcessor(IAnsiConsole console, CancellationToken cancellationToken) : ICatalogLeafProcessor
    {
        public Task<bool> ProcessPackageDeleteAsync(PackageDeleteCatalogLeaf leaf)
        {
            console.WriteLine($"{leaf.CommitTimestamp:O}: Found package delete leaf for {leaf.PackageId} {leaf.PackageVersion}.");
            return Task.FromResult(!cancellationToken.IsCancellationRequested);
        }

        public Task<bool> ProcessPackageDetailsAsync(PackageDetailsCatalogLeaf leaf)
        {
            console.WriteLine($"{leaf.CommitTimestamp:O}: Found package details leaf for {leaf.PackageId} {leaf.PackageVersion}.");
            return Task.FromResult(!cancellationToken.IsCancellationRequested);
        }
    }

    private sealed class InMemoryCursor : ICursor
    {
        public DateTimeOffset? Value { get; set; }

        public Task<DateTimeOffset?> GetAsync() => Task.FromResult(Value);

        public Task SetAsync(DateTimeOffset value)
        {
            Value = value;
            return Task.CompletedTask;
        }
    }
}
