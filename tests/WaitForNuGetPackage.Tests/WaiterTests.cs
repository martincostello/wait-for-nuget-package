// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

public class WaiterTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Application_Returns_Zero_If_Packages_In_Directory_Are_Published()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        string[] packageNames = ["Polly.Core", "Polly.Extensions"];
        var packageVersion = "8.6.6";

        var tempDir = Directory.CreateTempSubdirectory();
        using var client = new HttpClient();

        foreach (var packageName in packageNames)
        {
            var packageFile = Path.Combine(tempDir.FullName, $"{packageName}.{packageVersion}.nupkg");
            var packageUrl = $"https://www.nuget.org/api/v2/package/{packageName}/{packageVersion}";
            var packageBytes = await client.GetByteArrayAsync(packageUrl, cancellationToken);

            await File.WriteAllBytesAsync(packageFile, packageBytes, cancellationToken);
        }

        using var console = new TestConsole();

        try
        {
            // Act
            var actual = await Waiter.RunAsync(
                console,
                ["--directory", tempDir.FullName],
                cancellationToken);

            // Assert
            actual.ShouldBe(0);
        }
        finally
        {
            output.WriteLine(console.Output);
        }
    }

    [Fact]
    public async Task Application_Returns_Two_If_Cancelled()
    {
        // Arrange
        using var console = new TestConsole();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        try
        {
            // Act
            var actual = await Waiter.RunAsync(
                console,
                ["Polly"],
                cts.Token);

            // Assert
            actual.ShouldBe(2);
        }
        finally
        {
            output.WriteLine(console.Output);
        }
    }
}
