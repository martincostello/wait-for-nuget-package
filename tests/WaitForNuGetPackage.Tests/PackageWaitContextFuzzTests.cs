// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using FsCheck.Xunit;
using NuGet.Protocol.Catalog;
using NuGet.Versioning;
using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

[Properties(Arbitrary = [typeof(NuGetArbitraries)], MaxTest = 200)]
public static class PackageWaitContextFuzzTests
{
    [Property]
    public static async Task Process_Matches_Desired_Package_Ignoring_Case(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        int offset,
        bool verbose)
    {
        // Arrange
        using var console = new TestConsole().WithWideProfile();

        var settings = new WaitCommandSettings() { Packages = [$"{id.Value}@{version.Value}"], Verbose = verbose };
        var context = new PackageWaitContext(console, settings);

        (await context.DiscoverPackagesAsync(CancellationToken.None)).ShouldBeTrue();
        context.AllPublished.ShouldBeFalse();

        var item = CreateLeaf(NuGetArbitraries.MixCase(id.Value, offset), NuGetArbitraries.MixCase(version.Value, offset));

        // Act
        bool actual = context.Process(item);

        // Assert
        actual.ShouldBeTrue();
        context.AllPublished.ShouldBeTrue();
        context.ObservedPackages.ShouldBe([(item.PackageId, item.PackageVersion)]);

        console.Output.ShouldNotContain('\u001b');
        console.Output.ShouldContain("was published.");
    }

    [Property]
    public static async Task Process_Matches_Any_Version_When_No_Version_Is_Specified(
        GeneratedPackageId id,
        GeneratedPackageVersion version)
    {
        // Arrange
        using var console = new TestConsole().WithWideProfile();

        var settings = new WaitCommandSettings() { Packages = [id.Value] };
        var context = new PackageWaitContext(console, settings);

        (await context.DiscoverPackagesAsync(CancellationToken.None)).ShouldBeTrue();

        // Act
        bool actual = context.Process(CreateLeaf(id.Value, version.Value));

        // Assert
        actual.ShouldBeTrue();
        context.AllPublished.ShouldBeTrue();
    }

    [Property]
    public static async Task Process_Does_Not_Match_Other_Packages_And_Never_Throws(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        GeneratedPackageId otherId,
        GeneratedPackageVersion otherVersion,
        bool verbose)
    {
        // Arrange
        using var console = new TestConsole().WithWideProfile();

        var settings = new WaitCommandSettings() { Packages = [$"{id.Value}@{version.Value}"], Verbose = verbose };
        var context = new PackageWaitContext(console, settings);

        (await context.DiscoverPackagesAsync(CancellationToken.None)).ShouldBeTrue();

        bool expected =
            string.Equals(id.Value, otherId.Value, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(version.Value, otherVersion.Value, StringComparison.OrdinalIgnoreCase);

        // Act
        bool actual = context.Process(CreateLeaf(otherId.Value, otherVersion.Value));

        // Assert
        actual.ShouldBe(expected);
        context.AllPublished.ShouldBe(expected);
        console.Output.ShouldNotContain('\u001b');
    }

    [Property(MaxTest = 50)]
    public static async Task DiscoverPackagesAsync_Finds_Packages_In_Directories_And_Files(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        GeneratedPackageId otherId,
        GeneratedPackageVersion otherVersion,
        byte depth)
    {
        // Arrange
        using var console = new TestConsole().WithWideProfile();
        using var directory = new TemporaryDirectory();

        var nested = directory.Path;

        for (int i = 0; i < depth % 4; i++)
        {
            nested = Path.Combine(nested, $"directory-{i}");
        }

        var first = CreatePackage(nested, id.Value, version.Value);
        var second = CreatePackage(directory.Path, otherId.Value, otherVersion.Value);

        var viaDirectory = new WaitCommandSettings() { Directories = [directory.Path] };
        var viaFiles = new WaitCommandSettings() { Files = [first, second] };

        var expected = new[]
        {
            new DesiredNuGetPackage(id.Value, NuGetVersion.Parse(version.Value).ToNormalizedString()),
            new DesiredNuGetPackage(otherId.Value, NuGetVersion.Parse(otherVersion.Value).ToNormalizedString()),
        };

        // Act
        var fromDirectory = new PackageWaitContext(console, viaDirectory);
        var fromFiles = new PackageWaitContext(console, viaFiles);

        bool foundInDirectory = await fromDirectory.DiscoverPackagesAsync(CancellationToken.None);
        bool foundInFiles = await fromFiles.DiscoverPackagesAsync(CancellationToken.None);

        // Assert
        foundInDirectory.ShouldBeTrue();
        foundInFiles.ShouldBeTrue();

        foreach (var package in expected)
        {
            fromDirectory.DesiredPackages.ShouldContain(package);
            fromFiles.DesiredPackages.ShouldContain(package);
        }
    }

    private static CatalogLeafItem CreateLeaf(string id, string version)
    {
        return new()
        {
            Type = CatalogLeafType.PackageDetails,
            CommitId = Guid.NewGuid().ToString(),
            CommitTimestamp = new DateTimeOffset(2026, 9, 22, 8, 29, 32, TimeSpan.Zero),
            PackageId = id,
            PackageVersion = version,
        };
    }

    private static string CreatePackage(string directory, string id, string version)
    {
        Directory.CreateDirectory(directory);

        // Use a unique file name as different IDs and versions can otherwise map to the
        // same name (e.g. "A.1" + "2.3.1" and "A" + "1.2.3.1"), or to reserved device names on Windows.
        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.nupkg");

        using var stream = File.Create(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        var entry = archive.CreateEntry($"{id}.nuspec");

        using var writer = new StreamWriter(entry.Open());

        writer.Write(
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
              <metadata>
                <id>{id}</id>
                <version>{version}</version>
                <authors>Test</authors>
                <description>Test</description>
              </metadata>
            </package>
            """);

        return path;
    }

    private static TestConsole WithWideProfile(this TestConsole console)
    {
        console.Profile.Width = 400;
        return console;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"fuzz-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (IOException)
            {
                // Ignore
            }
        }
    }
}
