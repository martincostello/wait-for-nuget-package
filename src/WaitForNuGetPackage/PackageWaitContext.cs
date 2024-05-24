// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
using Spectre.Console;
using PublishedPackage = (string Id, string Version);

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class PackageWaitContext(IAnsiConsole console, WaitCommandSettings settings)
{
    private readonly HashSet<DesiredNuGetPackage> _desired = GetDesiredPackages(settings.Packages);
    private readonly List<PublishedPackage> _observed = [];
    private readonly bool _verbose = settings.Verbose is true;

    public bool AllPublished => _desired.Count is 0;

    public IReadOnlyList<PublishedPackage> ObservedPackages => _observed;

    public bool Process(ICatalogLeafItem item)
    {
        bool found = _desired.RemoveWhere((p) => p.Equals(item)) > 0;

        if (found)
        {
            _observed.Add((item.PackageId, item.PackageVersion));
        }

        if (found || _verbose)
        {
            var textColor = found ? Color.Silver : Color.Grey;
            var packageNameColor = found ? Color.Lime : Color.Grey;
            var packageVersionColor = found ? Color.Aqua : Color.Grey;

            var timestamp = $"[{item.CommitTimestamp:u}]";
            console.MarkupLineInterpolated(
                $"[{textColor}]{timestamp} {Emoji.Known.Package} Package [{packageNameColor}]{item.PackageId}[/]@[{packageVersionColor}]{item.PackageVersion}[/] was published.[/]");
        }

        return found;
    }

    private static HashSet<DesiredNuGetPackage> GetDesiredPackages(IEnumerable<string> packageNames)
    {
        HashSet<DesiredNuGetPackage> packages = [];

        foreach (var nameAndMaybeVersion in packageNames)
        {
            var name = nameAndMaybeVersion;
            int index = name.IndexOf('@', StringComparison.Ordinal);

            DesiredNuGetPackage desired;

            if (index > 0)
            {
                var version = name[(index + 1)..];
                name = name[..index];

                desired = new(name, version);
            }
            else
            {
                desired = new(name);
            }

            packages.Add(desired);
        }

        return packages;
    }
}
