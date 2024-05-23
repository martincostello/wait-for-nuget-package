﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

public class WaiterTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Application_Runs_With_No_Errors()
    {
        // Arrange
        using var console = new TestConsole();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            // Act
            var actual = await Waiter.RunAsync(
                console,
                ["Polly"],
                cts.Token);

            // Assert
            actual.ShouldBe(0);
        }
        finally
        {
            output.WriteLine(console.Output);
        }
    }
}
