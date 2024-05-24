// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using NuGet.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

/// <summary>
/// A class representing the settings for <see cref="WaitCommand"/>. This class cannot be inherited.
/// </summary>
internal sealed class WaitCommandSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets an optional value indicating whether to disable the logo.
    /// </summary>
    [CommandOption("-q|--no-logo")]
    [Description("Suppresses the logo.")]
    public bool? NoLogo { get; set; }

    /// <summary>
    /// Gets or sets the packages to wait on being published.
    /// </summary>
    [CommandArgument(0, "[package]")]
    [Description("The name(s), including an optional version, to wait for new versions to be published.")]
    public string[] Packages { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional timeout to wait for the package(s) to be published.
    /// </summary>
    [CommandOption("-t|--timeout")]
    [Description("The period of time to wait for the package(s) to be published.")]
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets an optional value indicating whether to enable verbose logging.
    /// </summary>
    [CommandOption("--verbose")]
    [Description("Enables verbose logging.")]
    public bool? Verbose { get; set; }

    /// <inheritdoc/>
    public override ValidationResult Validate()
    {
        if (Packages.Length < 1)
        {
            return ValidationResult.Error("At least one NuGet package to wait for must be specified.");
        }

        foreach (var package in Packages)
        {
            int index = package.IndexOf('@', StringComparison.Ordinal);

            if (index > 0)
            {
                var version = package[(index + 1)..];
                if (!NuGetVersion.TryParse(version, out _))
                {
                    return ValidationResult.Error($"The version '{version}' for the package '{package[0..index]}' is not a valid NuGet package version.");
                }
            }
        }

        if (Timeout is { } timeout && timeout <= TimeSpan.Zero)
        {
            return ValidationResult.Error("The timeout must be a positive duration.");
        }

        return ValidationResult.Success();
    }
}
