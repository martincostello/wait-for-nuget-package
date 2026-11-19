// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using FsCheck.Xunit;
using NuGet.Versioning;
using Spectre.Console.Testing;

namespace MartinCostello.WaitForNuGetPackage;

[Properties(Arbitrary = [typeof(NuGetArbitraries)], MaxTest = 500)]
public static class PackageSpecificationFuzzTests
{
    [Property]
    public static async Task Package_With_Version_Is_Valid_And_Parsed_Consistently(
        GeneratedPackageId id,
        GeneratedPackageVersion version)
    {
        // Arrange
        var settings = new WaitCommandSettings() { Packages = [$"{id.Value}@{version.Value}"] };

        // Act
        var validation = settings.Validate();
        var desired = await Discover(settings);

        // Assert
        validation.Successful.ShouldBeTrue(validation.Message);
        desired.ShouldBe([new DesiredNuGetPackage(id.Value, version.Value)]);
    }

    [Property]
    public static async Task Package_Without_Version_Is_Valid_And_Matches_Any_Version(GeneratedPackageId id)
    {
        // Arrange
        var settings = new WaitCommandSettings() { Packages = [id.Value] };

        // Act
        var validation = settings.Validate();
        var desired = await Discover(settings);

        // Assert
        validation.Successful.ShouldBeTrue(validation.Message);
        desired.ShouldBe([new DesiredNuGetPackage(id.Value, DesiredNuGetPackage.AnyVersion)]);
        desired.ShouldAllBe((p) => p.Version == DesiredNuGetPackage.AnyVersion);
    }

    [Property]
    public static void Package_With_Invalid_Version_Is_Rejected(
        GeneratedPackageId id,
        GeneratedPackageSpecification version)
    {
        if (NuGetVersion.TryParse(version.Value, out _))
        {
            return;
        }

        var settings = new WaitCommandSettings() { Packages = [$"{id.Value}@{version.Value}"] };

        var validation = settings.Validate();

        validation.Successful.ShouldBeFalse();
        validation.Message.ShouldNotBeNull();
        validation.Message.ShouldContain(id.Value);
    }

    [Property]
    public static async Task Validation_And_Parsing_Of_Arbitrary_Package_Specifications_Agree(
        GeneratedPackageSpecification[] specifications)
    {
        // Arrange
        var settings = new WaitCommandSettings() { Packages = [.. specifications.Select((p) => p.Value)] };

        // Act
        var validation = settings.Validate();

        // Assert
        if (specifications.Length is 0)
        {
            validation.Successful.ShouldBeFalse();
            return;
        }

        bool expected = specifications.All(IsValid);
        validation.Successful.ShouldBe(expected);

        if (expected)
        {
            // Everything that passes validation must be parsed without error,
            // and must only ever produce a wildcard or a parsable NuGet version.
            var desired = await Discover(settings);

            desired.ShouldNotBeEmpty();
            desired.Count.ShouldBeLessThanOrEqualTo(specifications.Length);

            foreach (var package in desired)
            {
                if (package.Version != DesiredNuGetPackage.AnyVersion)
                {
                    NuGetVersion.TryParse(package.Version, out _).ShouldBeTrue();
                }
            }
        }

        static bool IsValid(GeneratedPackageSpecification specification)
        {
            int index = specification.Value.IndexOf('@', StringComparison.Ordinal);
            return index < 1 || NuGetVersion.TryParse(specification.Value[(index + 1)..], out _);
        }
    }

    [Property]
    public static void Timeout_Is_Valid_If_And_Only_If_Positive(long ticks)
    {
        var timeout = TimeSpan.FromTicks(ticks);
        var settings = new WaitCommandSettings() { Packages = ["Package"], Timeout = timeout };

        settings.Validate().Successful.ShouldBe(timeout > TimeSpan.Zero);
    }

    [Property]
    public static void Since_Is_Valid_If_And_Only_If_Not_Negative(long ticks)
    {
        var since = TimeSpan.FromTicks(ticks);
        var settings = new WaitCommandSettings() { Packages = ["Package"], Since = since };

        settings.Validate().Successful.ShouldBe(since >= TimeSpan.Zero);
    }

    [Property]
    public static void Service_Index_Url_Validation_Matches_Uri_Parsing(GeneratedPackageSpecification url)
    {
        var settings = new WaitCommandSettings() { Packages = ["Package"], ServiceIndexUrl = url.Value };

        settings.Validate().Successful.ShouldBe(Uri.TryCreate(url.Value, UriKind.Absolute, out _));
    }

    private static async Task<IReadOnlyCollection<DesiredNuGetPackage>> Discover(WaitCommandSettings settings)
    {
        using var console = new TestConsole();
        var context = new PackageWaitContext(console, settings);

        (await context.DiscoverPackagesAsync(CancellationToken.None)).ShouldBeTrue();

        return [.. context.DesiredPackages];
    }
}
