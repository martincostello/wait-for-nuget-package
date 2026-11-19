// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A value that may or may not be a package specification, generated for use in property-based tests.
/// </summary>
/// <param name="Value">The raw specification.</param>
public sealed record GeneratedPackageSpecification(string Value)
{
    /// <inheritdoc/>
    public override string ToString() => $"\"{Value}\"";
}
