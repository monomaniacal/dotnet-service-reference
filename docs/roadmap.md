# Roadmap

Practices are adopted in tiers. Each tier has an entry criterion, a CI job group, and an issue
label. This page describes the tiers and themes; the items themselves are GitHub issues, so
nothing is listed twice. Each link below is a live query.

## Tier 0: always on

Entry criterion: the repo exists. Required on every pull request by branch protection.

Landed: shared build settings and analyzers, central package management and lock files,
NuGet audit, tool manifest, pre-commit hooks, the `tier0-*` CI jobs, coverage on changed
lines, typed configuration with user secrets, the committed OpenAPI document, the API
reference UI, the dependency register, Dependabot, contribution templates. Each has a
page in `docs/practices/`.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-0

## Tier 1: before first deploy

Entry criterion: something will consume the service outside the developer loop.

Themes: mutation testing as the test-quality gate, CodeQL, integration tests in CI, breaking-
change detection on the OpenAPI document, DAST, readiness endpoint, the `dotnet new` template.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-1

## Tier 2: operating in production

Entry criterion: the service is deployed and someone is on call for it.

Themes: architecture tests, observability, container scanning and SBOM, reusable workflows.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-2

## Gated

Items with a named trigger that has not fired. Each issue body states the trigger.

https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Agated

## Themes

`theme:build`, `theme:testing`, `theme:security`, `theme:api`, `theme:config`,
`theme:governance`, `theme:template`, `theme:dx`. Query any theme with `label:theme:<name>`.

## Priority

Tier first, then whatever unblocks the most within a tier. The issue's "Depends on" field
carries the ordering.
