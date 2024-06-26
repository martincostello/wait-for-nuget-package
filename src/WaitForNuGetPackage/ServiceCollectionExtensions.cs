﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
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
        services.AddSingleton(cancellationTokenSource);
        services.AddSingleton(console);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<NuGetRepository>();
        services.AddSingleton<PackageWaitContext>();
        services.AddSingleton<ICursor, InMemoryCursor>();

        services.AddHttpClient()
                .ConfigureHttpClientDefaults((builder) =>
                {
                    builder.ConfigureHttpClient((client) => client.DefaultRequestHeaders.UserAgent.Add(_userAgent));
                    builder.AddStandardResilienceHandler();
                });

        services.AddHttpClient<ISimpleHttpClient, SimpleHttpClient>();

        services.AddTransient<ICatalogClient, CatalogClient>();
        services.AddTransient<ICatalogLeafProcessor, CatalogLeafProcessor>();
        services.AddTransient<CatalogProcessor>();
        services.AddTransient<CatalogProcessorSettings>();

        services.AddSingleton((provider) =>
        {
            var timeProvider = provider.GetRequiredService<TimeProvider>();
            var utcNow = timeProvider.GetUtcNow();

            var waitSettings = provider.GetRequiredService<WaitCommandSettings>();

            var processorSettings = new CatalogProcessorSettings()
            {
                DefaultMinCommitTimestamp = utcNow.Add(-(waitSettings.Since ?? TimeSpan.Zero)),
                ExcludeRedundantLeaves = false,
            };

            if (waitSettings.ServiceIndexUrl is { } serviceIndexUrl)
            {
                processorSettings.ServiceIndexUrl = serviceIndexUrl;
            }
            else
            {
                waitSettings.ServiceIndexUrl = processorSettings.ServiceIndexUrl;
            }

            if (waitSettings.Timeout is { } timeout)
            {
                processorSettings.MaxCommitTimestamp = utcNow.Add(timeout);
            }

            return processorSettings;
        });

        services.AddLogging((builder) =>
        {
            var level =
                args.Contains("--verbose", StringComparer.OrdinalIgnoreCase) ?
                LogLevel.Debug :
                LogLevel.Warning;

            builder.AddConsole()
                   .AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("NuGet", LogLevel.Warning)
                   .AddFilter("Polly", LogLevel.Error)
                   .AddFilter("System", LogLevel.Warning)
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
