# NuGet audit

## What

Restore checks every package, direct and transitive, against the GitHub Advisory Database and
reports advisories of any severity as warnings. Warnings are errors, so a vulnerable package
fails the restore.

## Why

Dependency scanning with no tool to install, no service to sign up for, and no way to skip it:
it runs inside `dotnet restore`. Transitive mode matters because most vulnerable packages
arrive indirectly.

## Source

- NuGet audit: https://learn.microsoft.com/nuget/concepts/auditing-packages

## Enforced by

`dotnet restore` in every build, locally and in `tier0-build`.

## Opt out

Raise `NuGetAuditLevel` to `moderate`, `high`, or `critical` to ignore lower severities. Set
`NuGetAuditMode` to `direct` to skip transitive packages. To accept one advisory knowingly, add
a `<NuGetAuditSuppress Include="<advisory url>" />` item to `Directory.Packages.props` with the
reason in `docs/dependencies.md`.

## Go deeper

Dependabot security updates (`docs/practices/repo-settings.md`) open the fixing pull request
automatically.
