# Build settings and analyzers

## What

`Directory.Build.props` sets the compiler and analyzer options once for every project: nullable
reference types, implicit usings, warnings as errors, the `latest-recommended` analysis level,
and code-style enforcement in the build. `.editorconfig` holds the formatting and style rules
and the per-rule severities. `global.json` pins the SDK so every machine and CI runner compiles
with the same toolchain.

## Why

Lint that runs in the compiler costs nothing to install and cannot be skipped. One props file
means a new project inherits the rules by existing. Warnings as errors turns every analyzer
finding into a failed build, so nothing is triaged by hand later.

## Source

- Directory.Build.props: https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory
- Code analysis in .NET: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview
- Code-style rules and EditorConfig: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/code-style-rule-options
- global.json: https://learn.microsoft.com/dotnet/core/tools/global-json

## Enforced by

`dotnet build` (every project, every CI job that builds). `dotnet format --verify-no-changes` in
the pre-commit hook and the `tier0-format` job.

## Opt out

Lower `AnalysisLevel` to `latest-minimum` in `Directory.Build.props` to keep only the rules
Microsoft enables by default. Remove `EnforceCodeStyleInBuild` to stop style rules failing the
build. Either change loses the guarantee that CI and the editor agree.

Rules disabled in `.editorconfig` and why:

- `CA1707` in tests: underscores in test names are the naming convention.
- `CA1711` in tests: `ICollectionFixture` marker classes follow xUnit's own `*Collection` naming
  convention (e.g. `ConfigServiceCollection`); they do not implement a collection type themselves.

## Go deeper

`AnalysisLevel=latest-all` enables every rule and needs a triage pass. Roslynator or StyleCop
add rules Microsoft does not ship; neither is planned.
