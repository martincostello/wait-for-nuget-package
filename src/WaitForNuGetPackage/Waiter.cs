// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
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
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var services = new ServiceCollection().AddServices(args, console, cts);
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
}
