// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text;
using FsCheck;
using FsCheck.Fluent;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// Custom <see cref="Arbitrary{T}"/> implementations for NuGet package metadata.
/// </summary>
public static class NuGetArbitraries
{
    private const string AlphaNumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string Alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Characters that are interesting to the parsing of package specifications
    private const string SpecificationCharacters = "aZ09.-_+@*[]~ ";

    private static readonly char[] IdSeparators = ['.', '-', '_'];

    /// <summary>
    /// Gets an <see cref="Arbitrary{T}"/> that generates valid NuGet package IDs.
    /// </summary>
    /// <returns>
    /// The generator for package IDs.
    /// </returns>
    public static Arbitrary<GeneratedPackageId> PackageId()
    {
        var segment = String(AlphaNumeric, 1, 8);
        var separator = Gen.Elements(IdSeparators);

        var gen =
            from first in segment
            from rest in ListOf(separator.SelectMany(s => segment.Select(x => s + x)), 0, 3)
            select new GeneratedPackageId(first + string.Concat(rest));

        return Arb.From(gen);
    }

    /// <summary>
    /// Gets an <see cref="Arbitrary{T}"/> that generates valid NuGet package versions.
    /// </summary>
    /// <returns>
    /// The generator for package versions.
    /// </returns>
    public static Arbitrary<GeneratedPackageVersion> PackageVersion()
    {
        var number = Gen.Choose(0, 999);
        var label = String(Alpha, 1, 1).SelectMany(first => String(AlphaNumeric, 0, 6).Select(rest => first + rest));
        var prerelease = ListOf(label, 0, 3).Select(x => x.Count < 1 ? string.Empty : "-" + string.Join('.', x));
        var metadata = Gen.Elements(string.Empty, string.Empty, "+build", "+sha.5114f85");

        var gen =
            from major in number
            from minor in number
            from patch in number
            from revision in Gen.Elements<int?>(null, null, 0, 1, 7)
            from pre in prerelease
            from meta in metadata
            select new GeneratedPackageVersion(
                $"{major}.{minor}.{patch}{(revision is { } r ? $".{r}" : string.Empty)}{pre}{meta}");

        return Arb.From(gen);
    }

    /// <summary>
    /// Gets an <see cref="Arbitrary{T}"/> that generates strings that look vaguely like package specifications.
    /// </summary>
    /// <returns>
    /// The generator for package specifications.
    /// </returns>
    public static Arbitrary<GeneratedPackageSpecification> PackageSpecification()
    {
        var gen = String(SpecificationCharacters, 0, 24).Select(x => new GeneratedPackageSpecification(x));
        return Arb.From(gen);
    }

    /// <summary>
    /// Returns a copy of the specified string with its character casing altered in a mixed pattern.
    /// </summary>
    /// <param name="value">The value to alter the casing of.</param>
    /// <param name="offset">The offset to use to determine which characters to upper-case.</param>
    /// <returns>
    /// The mixed-case string.
    /// </returns>
    public static string MixCase(string value, int offset)
    {
        var builder = new StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            builder.Append(((i + offset) & 1) is 0 ? char.ToUpperInvariant(value[i]) : char.ToLowerInvariant(value[i]));
        }

        return builder.ToString();
    }

    private static Gen<string> String(string alphabet, int minLength, int maxLength)
    {
        return
            from length in Gen.Choose(minLength, maxLength)
            from chars in Gen.ListOf(Gen.Elements(alphabet.ToCharArray()), length)
            select new string([.. chars]);
    }

    private static Gen<IReadOnlyList<T>> ListOf<T>(Gen<T> gen, int minLength, int maxLength)
    {
        return
            from length in Gen.Choose(minLength, maxLength)
            from items in Gen.ListOf(gen, length)
            select (IReadOnlyList<T>)[.. items];
    }
}
