// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NuGet.Packaging;
using NuGet.Protocol.Catalog;
using Spectre.Console;
using PublishedPackage = (string Id, string Version);

namespace MartinCostello.WaitForNuGetPackage;

internal sealed class PackageWaitContext(IAnsiConsole console, WaitCommandSettings settings)
{
    private readonly IAnsiConsole _console = console;
    private readonly HashSet<DesiredNuGetPackage> _desired = [];
    private readonly HashSet<DesiredNuGetPackage> _pending = [];
    private readonly List<PublishedPackage> _observed = [];
    private readonly WaitCommandSettings _settings = settings;

    public bool AllPublished => _pending.Count is 0;

    public IReadOnlySet<DesiredNuGetPackage> DesiredPackages => _desired;

    public IReadOnlyList<PublishedPackage> ObservedPackages => _observed;

    public async Task<bool> DiscoverPackagesAsync(CancellationToken cancellationToken)
    {
        _desired.AddRange(GetDesiredPackages(_settings.Packages));

        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var files = new HashSet<string>(comparer);

        foreach (var fileName in _settings.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path;

            try
            {
                path = Path.GetFullPath(fileName);
                files.Add(path);
            }
            catch (Exception ex)
            {
                _console.MarkupLine($"[{Color.Yellow}]{Emoji.Known.Warning} Failed to resolve file path {Link(fileName)}: {Markup.Escape(ex.Message)}[/]");
                _console.WriteLine();

                return false;
            }

            if (!File.Exists(path))
            {
                _console.MarkupLine($"[{Color.Yellow}]{Emoji.Known.Warning} NuGet package file {Link(path)} could not be found.[/]");
                _console.WriteLine();

                return false;
            }
        }

        foreach (var directory in _settings.Directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = directory;

            try
            {
                path = Path.GetFullPath(directory);

                foreach (var file in Directory.EnumerateFiles(path, "*.nupkg", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    files.Add(file);
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _console.MarkupLine($"[{Color.Yellow}]{Emoji.Known.Warning} Failed to enumerate files in directory {Link(path)}: {Markup.Escape(ex.Message)}[/]");
                _console.WriteLine();

                return false;
            }
        }

        if (files.Count > 0)
        {
            _desired.UnionWith(await GetPackageMetadataAsync(files, cancellationToken));
        }

        _pending.UnionWith(_desired);

        if (_desired.Count < 1)
        {
            _console.MarkupLine($"[{Color.Yellow}]{Emoji.Known.Warning} No packages specified or found to wait for.[/]");
            _console.WriteLine();

            return false;
        }

        return true;
    }

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

        if (found || _settings.Verbose is true)
        {
            var textColor = found ? Color.Silver : Color.Grey;
            var packageNameColor = found ? Color.Lime : Color.Grey;
            var packageVersionColor = found ? Color.Aqua : Color.Grey;

            // Ideally the package URL would come from the item so that we can be sure it points to the right registry
            var packageUrl = $"https://www.nuget.org/packages/{item.PackageId}/{item.PackageVersion}";

            var packageId = Markup.Escape(item.PackageId);
            var packageVersion = Markup.Escape(item.PackageVersion);

            var package = $"[{packageNameColor}]{packageId}[/][{textColor}]@[/][{packageVersionColor}]{packageVersion}[/]";

            if (_console.Profile.Capabilities.Links)
            {
                package = $"[link={packageUrl}]{package}[/]";
            }

            _console.MarkupLine($"[{textColor}][[{item.CommitTimestamp:u}]] {Emoji.Known.Package} Package {package} was published.[/]");
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

    private static async Task<IReadOnlyList<DesiredNuGetPackage>> GetPackageMetadataAsync(
        HashSet<string> packages,
        CancellationToken cancellationToken)
    {
        var result = new List<DesiredNuGetPackage>();

        foreach (var fileName in packages)
        {
            using var stream = File.OpenRead(fileName);
            using var reader = new PackageArchiveReader(stream);

            var identity = await reader.GetIdentityAsync(cancellationToken);

            result.Add(new DesiredNuGetPackage(identity.Id, identity.Version.ToNormalizedString()));
        }

        return result;
    }

    private static bool TryCreateFileUri(string path, out string uri)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var result) && result.IsFile)
        {
            uri = result.AbsoluteUri;
            return true;
        }

        uri = string.Empty;
        return false;
    }

    private string Link(string path)
    {
        var escaped = Markup.Escape(path);

        if (_console.Profile.Capabilities.Links && TryCreateFileUri(path, out var uri))
        {
            return $"[link={Markup.Escape(uri)}]{escaped}[/]";
        }

        return escaped;
    }
}
