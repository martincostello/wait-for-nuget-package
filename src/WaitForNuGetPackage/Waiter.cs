// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Catalog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// Waits for a new version of a NuGet package to be published.
/// </summary>
internal static class Waiter
{
    //// See https://github.com/NuGet/Samples/blob/ec30a2b7c54c2d09e5a476444a2c7a8f2f289d49/CatalogReaderExample

    public static readonly string Version = typeof(Waiter).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    private static readonly ProductInfoHeaderValue _userAgent = CreateUserAgent();

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
        var services = new ServiceCollection();

        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton(TimeProvider.System);

        services.AddHttpClient()
                .ConfigureHttpClientDefaults((builder) =>
                {
                    builder.ConfigureHttpClient((client) => client.DefaultRequestHeaders.UserAgent.Add(_userAgent));
                    builder.AddStandardResilienceHandler();
                });

        services.AddHttpClient<ISimpleHttpClient, SimpleHttpClient>();
        services.AddTransient<ICatalogClient, CatalogClient>();
        services.AddTransient<ICatalogLeafProcessor, CatalogLeafProcessor>();
        services.AddSingleton<ICursor, InMemoryCursor>();

        services.AddSingleton<CatalogProcessor>();
        services.AddSingleton<CatalogProcessorSettings>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        services.AddSingleton(cts);

        // TODO Wire this up to DI nicely
        // TODO Allow the service index to be specified
        var settings = new CatalogProcessorSettings()
        {
            DefaultMinCommitTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
            ExcludeRedundantLeaves = false,
        };

        services.AddSingleton(settings);

        services.AddLogging((builder) =>
        {
            var level =
                args.Contains("--verbose", StringComparer.OrdinalIgnoreCase) ?
                LogLevel.Debug :
                LogLevel.Warning;

            builder.AddConsole()
                   .SetMinimumLevel(level);
        });

        var registrar = new TypeRegistrar(services);

        var app = new CommandApp<WaitCommand>(registrar);

        app.Configure((config) =>
        {
            config.AddExample(["MyCompany.MyProduct", "MyCompany.MyOtherProduct@1.2.3", "--timeout", "00:15:00"]);
            config.ConfigureConsole(console);
            config.SetInterceptor(new TimeoutInterceptor(cts));
            config.UseAssemblyInformationalVersion();
        });

        return await app.RunAsync(args);
    }

    private static ProductInfoHeaderValue CreateUserAgent()
    {
        var productVersion = Version;

        // Truncate the Git commit SHA to 7 characters, if present
        int indexOfPlus = productVersion.IndexOf('+', StringComparison.Ordinal);

        if (indexOfPlus > -1 && indexOfPlus < productVersion.Length - 1)
        {
            string hash = productVersion[(indexOfPlus + 1)..];

            if (hash.Length > 7)
            {
                productVersion = productVersion[..(indexOfPlus + 8)];
            }
        }

        return new ProductInfoHeaderValue("WaitForNuGetPackage", productVersion);
    }
}
