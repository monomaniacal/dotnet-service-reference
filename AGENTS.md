# AGENTS.md

Working agreements for AI assistants in this repo. Read `docs/principles.md` first once it
exists; until then this file is the rule set.

## What this repo is

A public reference .NET service. It demonstrates engineering practices that are enforced by
tooling and explained in `docs/`, and it is the source of a `dotnet new` template. The design
and the prioritised backlog are in `docs/specs/2026-09-13-reference-repo-design.md`.

## Rules

- **Failing test first.** Write the test, watch it fail, then write the code. Say which test
  drove a change in the PR.
- **No explanatory comments in code.** Rationale goes in `docs/practices/` or an ADR in
  `docs/decisions/`. XML doc comments are allowed only where they feed the OpenAPI document.
- **Microsoft-first.** Prefer what ships with the SDK, ASP.NET Core, or an official Microsoft
  package, and cite the Microsoft Learn page. Use a third-party tool only where Microsoft has no
  equivalent, and say so.
- **Nothing speculative.** Every addition names the condition that makes it necessary now. If
  the trigger has not fired, it is a backlog item, not code.
- **Every package justifies itself.** A new package needs a row in `docs/dependencies.md`
  saying why it is here and what it replaces. CI fails without it.
- **Versions are pinned once.** Never add a version to a `.csproj` once central package
  management is in place.

## Git

- Default branch is `main`. Work on feature branches; open a PR; Daniel merges.
- Assistants may commit and push feature branches. Small, reviewable commits.
- Never commit build output, `TestResults/`, `.vs/`, `.env`, or `*.user` files.
