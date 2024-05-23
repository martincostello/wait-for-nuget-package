# WaitForNuGetPackage

<!--
[![NuGet][package-badge]][package-download]
-->

[![Build status][build-badge]][build-status]

<!--
[![codecov][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]
-->

## Introduction

Waits for a new version of a NuGet package to be published.

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

[build-badge]: https://github.com/martincostello/wait-for-nuget-package/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush
[build-status]: https://github.com/martincostello/wait-for-nuget-package/actions/workflows/build.yml/badge.svg?branch=main&event=push "Continuous Integration for this project"
<!--
[coverage-badge]: https://codecov.io/gh/martincostello/wait-for-nuget-package/branch/main/graph/badge.svg
[coverage-report]: https://codecov.io/gh/martincostello/wait-for-nuget-package "Code coverage report for this project"
-->
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[issues]: https://github.com/martincostello/wait-for-nuget-package/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
<!--
[package-badge]: https://buildstats.info/nuget/MartinCostello.WaitForNuGetPackage?includePreReleases=false
[package-download]: https://www.nuget.org/packages/MartinCostello.WaitForNuGetPackage "Download MartinCostello.WaitForNuGetPackage from NuGet"
-->
[repo]: https://github.com/martincostello/wait-for-nuget-package "This project on GitHub.com"
<!--
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/wait-for-nuget-package/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/wait-for-nuget-package "OpenSSF Scorecard for this project"
-->
