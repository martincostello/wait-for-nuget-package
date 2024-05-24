// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Catalog;
using Spectre.Console;

namespace MartinCostello.WaitForNuGetPackage;

internal static class ServiceCollectionExtensions
{
    private static readonly ProductInfoHeaderValue _userAgent = CreateUserAgent();

    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IReadOnlyCollection<string> args,
        IAnsiConsole console,
        CancellationTokenSource cancellationTokenSource)
    {
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
        services.AddSingleton<PackageWaitContext>();

        services.AddSingleton(cancellationTokenSource);

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
                   .AddFilter("NuGet", LogLevel.Warning)
                   .AddFilter("Polly", LogLevel.Error)
                   .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning)
                   .SetMinimumLevel(level);
        });

        return services;
    }

    private static ProductInfoHeaderValue CreateUserAgent()
    {
        var version = Waiter.Version;

        // Truncate the Git commit SHA to 7 characters, if present
        int indexOfPlus = version.IndexOf('+', StringComparison.Ordinal);

        if (indexOfPlus > -1 && indexOfPlus < version.Length - 1)
        {
            string hash = version[(indexOfPlus + 1)..];

            if (hash.Length > 7)
            {
                version = version[..(indexOfPlus + 8)];
            }
        }

        return new ProductInfoHeaderValue("WaitForNuGetPackage", version);
    }
}
