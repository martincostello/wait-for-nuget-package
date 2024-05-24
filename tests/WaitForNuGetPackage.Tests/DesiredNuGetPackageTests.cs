// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;

namespace MartinCostello.WaitForNuGetPackage;

public static class DesiredNuGetPackageTests
{
    public static TheoryData<string, string, string?, string, bool> EqualsTestCases()
    {
        return new()
        {
            { "Package", "*", null, string.Empty, false },
            { "Package", "1.0.0", "Other", "1.0.0", false },
            { "Package", "*", "Other", "*", false },
            { "Package", "*", "Package", "*", true },
            { "package", "*", "PACKAGE", "*", true },
            { "package", "1.0.0", "PACKAGE", "1.0.0", true },
            { "Package", "1.0.0", "Package", "1.0.0", true },
            { "Package", "1.0.0-beta.1", "Package", "1.0.0-beta.1", true },
            { "Package", "1.0.0-beta.1", "Package", "1.0.0-beta.2", false },
            { "Package", "1.0.0-beta.1", "Package", "1.0.0", false },
            { "Package", "1.0.0", "Package", "2.0.0", false },
            { "Package", "*", "Package", "1.0.0", true },
            { "Package", "1.0.0", "Package", "*", true },
        };
    }

    [Theory]
    [MemberData(nameof(EqualsTestCases))]
    public static void Equals_Returns_Correct_Result_For_Desired_Package(
        string id,
        string version,
        string? otherId,
        string otherVersion,
        bool expected)
    {
        // Arrange
        var target = new DesiredNuGetPackage(id, version);
        var other = CreateDesired(otherId, otherVersion);

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(EqualsTestCases))]
    public static void Equals_Returns_Correct_Result_For_Catalog_Leaf_Item(
        string id,
        string version,
        string? otherId,
        string otherVersion,
        bool expected)
    {
        // Arrange
        var target = new DesiredNuGetPackage(id, version);
        var other = CreateLeaf(otherId, otherVersion);

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public static void Equals_Returns_True_For_Self()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");

        // Act
        bool actual = target.Equals(target);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static void Equals_Returns_False_When_Other_Is_Not_DesiredNuGetPackage()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");
        object? other = new();

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public static void GetHashCode_Returns_Same_Value_For_Same_Objects()
    {
        // Arrange
        DesiredNuGetPackage target = new("Package", "1.0.0");
        DesiredNuGetPackage other = new("Package", "1.0.0");

        // Act
        int hashCode1 = target.GetHashCode();
        int hashCode2 = other.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
    }

    [Fact]
    public static void GetHashCode_Returns_Different_Value_For_Different_Objects()
    {
        // Arrange
        DesiredNuGetPackage target = new("Package", "1.0.0");
        DesiredNuGetPackage other = new("Package", "2.0.0");

        // Act
        int hashCode1 = target.GetHashCode();
        int hashCode2 = other.GetHashCode();

        // Assert
        hashCode1.ShouldNotBe(hashCode2);
    }

    private static DesiredNuGetPackage? CreateDesired(string? id, string version = "*")
    {
        return id is null ? null : new(id, version);
    }

    private static CatalogLeafItem? CreateLeaf(string? id, string version = "*")
    {
        return id is null ? null : new CatalogLeafItem() { PackageId = id, PackageVersion = version };
    }
}
