// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A NuGet package version generated for use in property-based tests.
/// </summary>
/// <param name="Value">The package version.</param>
public sealed record GeneratedPackageVersion(string Value)
{
    /// <inheritdoc/>
    public override string ToString() => Value;
}
