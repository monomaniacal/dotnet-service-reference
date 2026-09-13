# Dependabot

## What

Weekly grouped pull requests for NuGet packages and GitHub Actions. NuGet updates land in
`Directory.Packages.props` and the lock files together. Action updates bump the pinned SHA and
the version comment.

## Why

Pinned versions only stay safe if something proposes the bump. One grouped pull request a week
is reviewable; one per package is noise that gets ignored.

## Source

- Dependabot version updates: https://docs.github.com/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file
- NuGet central package management support: the same page, `nuget` ecosystem

## Enforced by

Nothing blocks; each Dependabot pull request runs the Tier 0 checks like any other.

## Opt out

Delete `.github/dependabot.yml`. NuGet audit still fails the build on known vulnerabilities;
you lose the proposed fix.

## Go deeper

Enable auto-merge for grouped patch updates once the Tier 1 mutation gate is in place.
