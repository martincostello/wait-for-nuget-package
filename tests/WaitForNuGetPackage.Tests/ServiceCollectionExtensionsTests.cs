// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

public static class ServiceCollectionExtensionsTests
{
    [Fact]
    public static void Service_Registrations_Are_Valid()
    {
        // Arrange
        using var console = new TestConsole();
        using var cancellationTokenSource = new CancellationTokenSource();

        var fixture = new TypeRegistrarBaseTests(() =>
        {
            var services = new ServiceCollection().AddServices(["--verbose"], console, cancellationTokenSource);
            return new TypeRegistrar(services);
        });

        // Act and Assert
        Should.NotThrow(fixture.RunAllTests);
    }
}
