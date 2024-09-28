# WaitForNuGetPackage ⌛📦

[![NuGet][package-badge-version]][package-download]
[![NuGet Downloads][package-badge-downloads]][package-download]

[![Build status][build-badge]][build-status]
[![codecov][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]

## Introduction

Waits for a new version of a NuGet package to be published.

## Quick Start

To install the tool and wait for a NuGet package to be published
and be available for package restore and visible in search results,
run the following commands:

```console
dotnet tool install --global MartinCostello.WaitForNuGetPackage

# Create a NuGet package and publish it to NuGet.org
dotnet pack MyPackage --output .
dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json

# Wait for the package to be published and indexed
dotnet wait-for-package MyPackage
```

### Examples

> Wait for a new version of a package to be published

```console
dotnet wait-for-package MyPackage
```

> Wait for a new version of a multiple packages to be published

```console
dotnet wait-for-package MyPackage.Core MyPackage.Data MyPackage.UI
```

> Wait for a specific version of a package to be published

```console
dotnet wait-for-package MyPackage@1.2.3
```

> Wait no more than 15 minutes for a new version of a package to be published

```console
dotnet wait-for-package MyPackage --timeout 00:15:00
```

> Wait for any new version of a package to have be published within the last 30 minutes

```console
dotnet wait-for-package MyPackage --since 00:30:00
```

> Wait for a new version of a package to be published to a custom NuGet feed
> [!NOTE]
> Your custom NuGet feed must implement the [NuGet Catalog resource](https://learn.microsoft.com/nuget/api/catalog-resource).

```console
dotnet wait-for-package MyPackage --service-index https://corp.local/nuget/index.json
```

### Options

```console
> dotnet wait-for-package --help
USAGE:
    dotnet wait-for-package [package-id] [OPTIONS]

EXAMPLES:
    dotnet wait-for-package MyCompany.MyProduct
    dotnet wait-for-package MyCompany.MyProduct@1.2.3
    dotnet wait-for-package MyCompany.MyProduct MyCompany.MyOtherProduct@1.2.3 --timeout 00:15:00

ARGUMENTS:
    [package-id]    The package ID(s), including an optional version, to wait for new versions to be published

OPTIONS:
                           DEFAULT
    -h, --help                         Prints help information
    -v, --version                      Prints version information
    -q, --no-logo                      Suppresses the logo
    -i, --service-index                The NuGet service index URL to use
    -s, --since            00:05:00    The period of time before now to include when searching for the published
                                       package(s)
    -t, --timeout                      The period of time to wait for the package(s) to be published
        --verbose                      Enables verbose logging
```

## Building and Testing

Compiling the application yourself requires Git and the [.NET SDK][dotnet-sdk] to be installed.

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/wait-for-nuget-package.git
cd wait-for-nuget-package
./build.ps1
```

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/wait-for-nuget-package.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[build-badge]: https://github.com/martincostello/wait-for-nuget-package/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/wait-for-nuget-package/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush "Continuous Integration for this project"
[coverage-badge]: https://codecov.io/gh/martincostello/wait-for-nuget-package/branch/main/graph/badge.svg
[coverage-report]: https://codecov.io/gh/martincostello/wait-for-nuget-package "Code coverage report for this project"
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[issues]: https://github.com/martincostello/wait-for-nuget-package/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[package-badge-downloads]: https://img.shields.io/nuget/dt/MartinCostello.WaitForNuGetPackage?logo=nuget&label=Downloads&color=blue
[package-badge-version]: https://img.shields.io/nuget/v/MartinCostello.WaitForNuGetPackage?logo=nuget&label=Latest&color=blue
[package-download]: https://www.nuget.org/packages/MartinCostello.WaitForNuGetPackage "Download MartinCostello.WaitForNuGetPackage from NuGet"
[repo]: https://github.com/martincostello/wait-for-nuget-package "This project on GitHub.com"
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/wait-for-nuget-package/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/wait-for-nuget-package "OpenSSF Scorecard for this project"
