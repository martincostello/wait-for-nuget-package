# Coding Agent Instructions

This file provides guidance to coding agents when working with code in this repository.

## Build and test commands

- Preferred full validation command: `pwsh -File .\build.ps1`
  - This is the repository's canonical local/CI entry point from `README.md`, `.github/CONTRIBUTING.md`, and `.github/workflows/build.yml`.
  - It bootstraps the exact SDK from `global.json` if needed, packs `src\WaitForNuGetPackage\WaitForNuGetPackage.csproj`, and runs the test suite unless `-SkipTests` is passed.
- Build/package without tests: `pwsh -File .\build.ps1 -SkipTests`
- Run the full test project directly: `dotnet test .\tests\WaitForNuGetPackage.Tests\WaitForNuGetPackage.Tests.csproj --configuration Release`
- Run a single test: `dotnet test .\tests\WaitForNuGetPackage.Tests\WaitForNuGetPackage.Tests.csproj --configuration Release --filter "FullyQualifiedName~DesiredNuGetPackageTests.Equals_Returns_True_For_Self" -p:CollectCoverage=false`
  - The test project enables coverlet thresholds by default, so filtered runs should disable coverage collection or they can fail even when the selected test passes.
- There is no separate lint script. StyleCop analyzers and the repository ruleset are enforced through the normal .NET build/test flow, and contributors are expected to keep `build.ps1` warning-free.

## High-level architecture

- This repository ships a .NET global tool named `dotnet-wait-for-package` from `src\WaitForNuGetPackage`.
- CLI flow is:
  - `Program.cs` sets up Windows Terminal progress integration, wires Ctrl+C cancellation, and delegates to `Waiter.RunAsync(...)`.
  - `Waiter.cs` builds a `Spectre.Console.Cli` `CommandApp<WaitCommand>`, configures examples/help, and bridges Spectre DI to `Microsoft.Extensions.DependencyInjection` via `TypeRegistrar` and `TypeResolver`.
  - `ServiceCollectionExtensions.cs` is the composition root. It registers the console, cancellation token source, `TimeProvider`, `NuGetRepository`, `PackageWaitContext`, the in-memory NuGet catalog cursor, HTTP clients, logging, and `CatalogProcessorSettings`.
  - `WaitCommand.cs` owns CLI presentation: logo/version output, package table, spinner, timeout/cancellation handling, and final exit code.
- Package waiting is intentionally two-phase:
  - `NuGetRepository.AllPackagesArePublishedAsync()` first checks the search API to short-circuit if packages are already searchable.
  - If not, `CatalogProcessor` streams NuGet catalog leaves through `CatalogLeafProcessor`, which hands each item to `PackageWaitContext`.
  - After a package is observed in the catalog, `NuGetRepository.WaitForIndexAsync()` polls the search index every 10 seconds until the package is searchable, so success means "published and indexed", not just "seen in the catalog".
- `PackageWaitContext` is the state holder for one command invocation. It parses requested packages, tracks pending vs. observed packages, prints catalog observations, and decides when all requested packages have been found.

## Key conventions

- Package arguments use `PackageId` or `PackageId@Version`. Omitting `@Version` means "any version" via `DesiredNuGetPackage.AnyVersion = "*"`.
- Matching is case-insensitive for package IDs and supports wildcard version matching. `DesiredNuGetPackage` implements equality against both another desired package and `ICatalogLeafItem`, which is why matching logic stays compact.
- The NuGet catalog cursor is intentionally in-memory only (`InMemoryCursor`). Each run starts from the `--since` window; there is no persisted cursor state across executions.
- Logging behavior is configured from raw CLI args in `ServiceCollectionExtensions.AddServices(...)`. `--verbose` raises the minimum level to `Debug`; otherwise the app keeps framework noise low with category filters.
- Timeout behavior is wired through `TimeoutInterceptor`, which calls `CancelAfter(...)` on the shared `CancellationTokenSource` before command execution starts.
- Important exit codes used in tests and command flow:
  - `0`: requested packages were found and indexed
  - `2`: timeout or cancellation
  - `-1`: invalid usage/validation failure from the CLI layer
- Tests use xUnit + Shouldly and follow the `{Subject}_{Scenario}_{ExpectedResult}` naming pattern. Integration tests call `Program.Main(...)` directly; service registration is validated with `Spectre.Console.Cli.Testing`; console-driven tests use `Spectre.Console.Testing.TestConsole`.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
