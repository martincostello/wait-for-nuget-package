// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Protocol.Catalog;
using Spectre.Console;
using PublishedPackage = (string Id, string Version);

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class PackageWaitContext
{
    private readonly IAnsiConsole _console;
    private readonly HashSet<DesiredNuGetPackage> _desired;
    private readonly HashSet<DesiredNuGetPackage> _pending;
    private readonly List<PublishedPackage> _observed;
    private readonly bool _verbose;

    public PackageWaitContext(IAnsiConsole console, WaitCommandSettings settings)
    {
        _console = console;
        _desired = GetDesiredPackages(settings.Packages);
        _pending = [.. _desired];
        _observed = [];
        _verbose = settings.Verbose is true;
    }

    public bool AllPublished => _pending.Count is 0;

    public IReadOnlySet<DesiredNuGetPackage> DesiredPackages => _desired;

    public IReadOnlyList<PublishedPackage> ObservedPackages => _observed;

    public void MarkPublished(string id, string version)
    {
        _pending.RemoveWhere((p) => p.Id == id);
        _observed.Add((id, version));
    }

    public bool Process(ICatalogLeafItem item)
    {
        bool found = _pending.RemoveWhere((p) => p.Equals(item)) > 0;

        if (found)
        {
            _observed.Add((item.PackageId, item.PackageVersion));
        }

        if (found || _verbose)
        {
            var textColor = found ? Color.Silver : Color.Grey;
            var packageNameColor = found ? Color.Lime : Color.Grey;
            var packageVersionColor = found ? Color.Aqua : Color.Grey;

            // Ideally the package URL would come from the item so that we can be sure it points to the right registry
            var packageUrl = $"https://www.nuget.org/packages/{item.PackageId}/{item.PackageVersion}";

            var timestamp = $"[{item.CommitTimestamp:u}]";
            _console.MarkupLineInterpolated(
                $"[{textColor}]{timestamp} {Emoji.Known.Package} Package [link={packageUrl}][{packageNameColor}]{item.PackageId}[/]@[{packageVersionColor}]{item.PackageVersion}[/][/] was published.[/]");
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
