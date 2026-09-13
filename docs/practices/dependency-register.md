# Dependency register

## What

`docs/dependencies.md` has one row per package: why it is here, what it replaces, and which
tier introduced it. A file-based .NET app in `scripts/` compares the register with
`Directory.Packages.props` and fails CI on a missing or stale row.

## Why

"It might be useful later" is the most common way a codebase grows a dependency it does not
need. Writing the reason down at the moment of adding forces the question, and keeps the
answer next to the version pin for the next person.

## Source

- File-based apps (`dotnet run file.cs`): https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk#file-based-apps
- Principle: `docs/principles.md`, "YAGNI with a trigger"

## Enforced by

The "Every package has a row" step of `tier0-build`.

## Opt out

Remove the step from CI. The register becomes advisory.

## Go deeper

Extend the script to require a tier label per row to match the roadmap.
