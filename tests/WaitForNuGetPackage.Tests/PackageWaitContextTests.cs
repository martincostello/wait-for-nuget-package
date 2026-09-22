// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
using Spectre.Console;
using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

public static class PackageWaitContextTests
{
    [Fact]
    public static async Task Process_Writes_Published_Package_Without_Emitting_Raw_Ansi_Escape_Codes()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var console = new TestConsole();

        var settings = new WaitCommandSettings() { Packages = ["Package.Id@1.2.3"] };
        var context = new PackageWaitContext(console, settings);

        (await context.DiscoverPackagesAsync(cancellationToken)).ShouldBeTrue();

        var item = new CatalogLeafItem()
        {
            Type = CatalogLeafType.PackageDetails,
            CommitId = Guid.NewGuid().ToString(),
            CommitTimestamp = new DateTimeOffset(2026, 9, 22, 8, 29, 32, TimeSpan.Zero),
            PackageId = "Package.Id",
            PackageVersion = "1.2.3",
        };

        // Act
        bool actual = context.Process(item);

        // Assert
        actual.ShouldBeTrue();

        console.Output.ShouldNotContain('\u001b');
        console.Output.ShouldContain("Package.Id");
        console.Output.ShouldContain("1.2.3");
        console.Output.ShouldContain("was published.");
        console.Output.ShouldContain(Emoji.Known.Package);
    }

    [Fact]
    public static async Task DiscoverPackagesAsync_Reports_Missing_File_Without_Emitting_Raw_Ansi_Escape_Codes()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var console = new TestConsole();

        var missingFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nupkg");
        var settings = new WaitCommandSettings() { Files = [missingFile] };
        var context = new PackageWaitContext(console, settings);

        // Act
        bool actual = await context.DiscoverPackagesAsync(cancellationToken);

        // Assert
        actual.ShouldBeFalse();

        console.Output.ShouldNotContain('\u001b');
        console.Output.ShouldContain("could not be found.");
    }
}
