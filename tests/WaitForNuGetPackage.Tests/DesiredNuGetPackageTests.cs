// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

public static class DesiredNuGetPackageTests
{
    [Fact]
    public static void Equals_Returns_False_When_Other_Is_Null()
    {
        // Arrange
        DesiredNuGetPackage target = new("Package");
        DesiredNuGetPackage? other = null;

#pragma warning disable CA1508
        // Act
        bool actual = target.Equals(other);
#pragma warning restore CA1508

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public static void Equals_Returns_False_When_Id_Does_Not_Match()
    {
        // Arrange
        DesiredNuGetPackage target = new("first", "1.0.0");
        DesiredNuGetPackage other = new("second", "1.0.0");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public static void Equals_Returns_False_When_Id_Does_Not_Match_For_Any_Version()
    {
        // Arrange
        DesiredNuGetPackage target = new("first");
        DesiredNuGetPackage other = new("second");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public static void Equals_Returns_True_When_Versions_Are_AnyVersion()
    {
        // Arrange
        DesiredNuGetPackage target = new("first");
        DesiredNuGetPackage other = new("first");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static void Equals_Returns_True_When_Versions_Are_AnyVersion_And_Case_Differs()
    {
        // Arrange
        DesiredNuGetPackage target = new("first");
        DesiredNuGetPackage other = new("FIRST");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static void Equals_Returns_True_When_Versions_Are_Equal()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");
        DesiredNuGetPackage other = new("package", "1.0.0");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeTrue();
    }

    [Theory]
    [InlineData("*")]
    [InlineData("1.0.0")]
    public static void Equals_Returns_True_When_Versions_Are_Same(string version)
    {
        // Arrange
        DesiredNuGetPackage target = new("package", version);

        // Act
        bool actual = target.Equals(target);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static void Equals_Returns_False_When_Versions_Are_Different()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");
        DesiredNuGetPackage other = new("package", "2.0.0");

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [Theory]
    [InlineData("1.0.0", "*")]
    [InlineData("*", "1.0.0")]
    public static void Equals_Returns_True_When_One_Version_Is_AnyVersion(string first, string second)
    {
        // Arrange
        DesiredNuGetPackage target = new("package", first);
        DesiredNuGetPackage other = new("package", second);

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static void Equals_Returns_False_When_Other_Is_Not_DesiredNuGetPackage()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");
        object other = new();

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public static void GetHashCode_Returns_Same_Value_For_Same_Objects()
    {
        // Arrange
        DesiredNuGetPackage target = new("package", "1.0.0");
        DesiredNuGetPackage other = new("package", "1.0.0");

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
        DesiredNuGetPackage target = new("package", "1.0.0");
        DesiredNuGetPackage other = new("package", "2.0.0");

        // Act
        int hashCode1 = target.GetHashCode();
        int hashCode2 = other.GetHashCode();

        // Assert
        hashCode1.ShouldNotBe(hashCode2);
    }
}
