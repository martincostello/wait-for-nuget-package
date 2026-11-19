// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using FsCheck.Xunit;
using NuGet.Protocol.Catalog;

namespace MartinCostello.WaitForNuGetPackage;

[Properties(Arbitrary = [typeof(NuGetArbitraries)], MaxTest = 500)]
public static class DesiredNuGetPackageFuzzTests
{
    [Property]
    public static void Equals_Returns_True_For_Equivalent_Instances(GeneratedPackageId id, GeneratedPackageVersion version)
    {
        var target = new DesiredNuGetPackage(id.Value, version.Value);
        var other = new DesiredNuGetPackage(id.Value, version.Value);

        target.Equals(other).ShouldBeTrue();
    }

    [Property]
    public static void Equals_Is_Symmetric(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        GeneratedPackageId otherId,
        GeneratedPackageVersion otherVersion,
        bool anyVersion)
    {
        var target = new DesiredNuGetPackage(id.Value, anyVersion ? DesiredNuGetPackage.AnyVersion : version.Value);
        var other = new DesiredNuGetPackage(otherId.Value, otherVersion.Value);

        target.Equals(other).ShouldBe(other.Equals(target));
    }

    [Property]
    public static void Equals_Ignores_Case_Of_Id_And_Version(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        int offset)
    {
        var target = new DesiredNuGetPackage(id.Value, version.Value);

        var other = new DesiredNuGetPackage(
            NuGetArbitraries.MixCase(id.Value, offset),
            NuGetArbitraries.MixCase(version.Value, offset));

        target.Equals(other).ShouldBeTrue();
        other.Equals(target).ShouldBeTrue();
    }

    [Property]
    public static void Equals_Matches_Any_Version_For_The_Same_Id(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        int offset)
    {
        var wildcard = new DesiredNuGetPackage(id.Value);
        var specific = new DesiredNuGetPackage(NuGetArbitraries.MixCase(id.Value, offset), version.Value);

        wildcard.Version.ShouldBe(DesiredNuGetPackage.AnyVersion);
        wildcard.Equals(specific).ShouldBeTrue();
        specific.Equals(wildcard).ShouldBeTrue();
    }

    [Property]
    public static void Equals_Returns_False_For_Different_Ids(
        GeneratedPackageId id,
        GeneratedPackageId otherId,
        GeneratedPackageVersion version,
        bool anyVersion)
    {
        if (string.Equals(id.Value, otherId.Value, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var versionOrWildcard = anyVersion ? DesiredNuGetPackage.AnyVersion : version.Value;

        var target = new DesiredNuGetPackage(id.Value, versionOrWildcard);
        var other = new DesiredNuGetPackage(otherId.Value, versionOrWildcard);

        target.Equals(other).ShouldBeFalse();
    }

    [Property]
    public static void Equals_Returns_False_For_Different_Versions_Of_The_Same_Id(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        GeneratedPackageVersion otherVersion)
    {
        if (string.Equals(version.Value, otherVersion.Value, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var target = new DesiredNuGetPackage(id.Value, version.Value);
        var other = new DesiredNuGetPackage(id.Value, otherVersion.Value);

        target.Equals(other).ShouldBeFalse();
    }

    [Property]
    public static void Equals_Gives_Same_Result_For_Desired_Package_And_Catalog_Leaf_Item(
        GeneratedPackageId id,
        GeneratedPackageVersion version,
        GeneratedPackageId otherId,
        GeneratedPackageVersion otherVersion,
        bool anyVersion,
        bool sameId)
    {
        var target = new DesiredNuGetPackage(id.Value, anyVersion ? DesiredNuGetPackage.AnyVersion : version.Value);

        var desired = new DesiredNuGetPackage(sameId ? id.Value : otherId.Value, otherVersion.Value);
        var leaf = new CatalogLeafItem() { PackageId = desired.Id, PackageVersion = desired.Version };

        target.Equals(leaf).ShouldBe(target.Equals(desired));
    }

    [Property]
    public static void GetHashCode_Is_Stable_For_Equal_Values(GeneratedPackageId id, GeneratedPackageVersion version)
    {
        var target = new DesiredNuGetPackage(id.Value, version.Value);
        var other = new DesiredNuGetPackage(id.Value, version.Value);

        target.GetHashCode().ShouldBe(other.GetHashCode());
    }
}
