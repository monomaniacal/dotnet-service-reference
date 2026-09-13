# CI: Tier 0 jobs

## What

`.github/workflows/ci.yml` runs on every pull request and every push to `main`. Jobs are named
by tier so the required-check list on `main` reads as the roadmap: `tier0-format`,
`tier0-build`, `tier0-unit`, `tier0-secrets`. Restores run in locked mode. Actions are pinned
to commit SHAs. Superseded runs on the same branch are cancelled.

## Why

A pull request that passes Tier 0 is formatted, builds with every analyzer and audit rule
enabled, passes the unit tests with coverage on its changed lines, and adds no secret. That is
the definition of "always on". SHA pinning means a compromised tag cannot change what runs.

## Source

- GitHub Actions for .NET: https://learn.microsoft.com/dotnet/devops/github-actions-overview
- setup-dotnet: https://github.com/actions/setup-dotnet
- Pinning actions to a SHA: https://docs.github.com/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions#using-third-party-actions

## Enforced by

Branch protection on `main` requires every `tier0-*` job (`docs/practices/repo-settings.md`).

## Opt out

Remove a job from the workflow and from the required checks. Both changes are visible in a
pull request.

## Go deeper

Tier 1 adds `tier1-integration`, `tier1-openapi-diff`, and `tier1-dast` to the same workflow,
plus separate CodeQL and mutation workflows.
