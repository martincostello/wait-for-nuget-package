// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;

namespace MartinCostello.WaitForNuGetPackage;

internal sealed record DesiredNuGetPackage(
    string Id,
    string Version = DesiredNuGetPackage.AnyVersion) : IEquatable<DesiredNuGetPackage>, IEquatable<ICatalogLeafItem>
{
    public const string AnyVersion = "*";

    public bool Equals(ICatalogLeafItem? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!string.Equals(Id, other.PackageId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Version is AnyVersion ||
               string.Equals(Version, other.PackageVersion, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(DesiredNuGetPackage? other)
    {
        if (other is null || !string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (Version is AnyVersion || other.Version is AnyVersion)
        {
            return true;
        }

        return string.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
        => HashCode.Combine(Id, Version);
}
