# Central package management and lock files

## What

`Directory.Packages.props` is the only file that states a package version. Project files
reference packages by name. Transitive pinning makes every project resolve the same version of
a package that appears in the props file, even when it arrives indirectly. Each project commits
a `packages.lock.json`, and CI restores with `--locked-mode`.

## Why

One place to read, review, and bump versions. Transitive pinning removed the EF Core version
conflict that used to need an explicit reference in the integration-test project (ADR 0005).
Lock files make a restore reproducible: the same inputs give the same graph on every machine
and every CI run, and a pull request that changes the graph shows it in the diff.

## Source

- Central package management: https://learn.microsoft.com/nuget/consume-packages/central-package-management
- Lock files: https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#locking-dependencies

## Enforced by

Restore fails on a `Version` attribute in a project file (`NU1008`) and on a lock file that no
longer matches (`NU1004`) when the `tier0-build` job runs `dotnet restore --locked-mode`.

## Opt out

Remove `RestorePackagesWithLockFile` and delete the lock files to lose reproducible restores.
Set `CentralPackageTransitivePinningEnabled` to `false` to let transitive versions float; the
EF Core conflict returns.

## Go deeper

Dependabot (`docs/practices/dependabot.md`) updates the props file and the lock files together.
