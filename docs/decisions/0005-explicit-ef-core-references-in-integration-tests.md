# 0005. The integration-test project references EF Core explicitly

Status: accepted, 2026-09-09. To be superseded by central package management (backlog item 3).

## Context

The API project gets EF Core transitively through the Npgsql provider, floored to 10.0.11 by
`Microsoft.EntityFrameworkCore.Design`. That floor is not propagated across the project
reference, so the integration-test project resolves the provider's declared minimum instead and
fails with CS1705 as soon as it touches `ConfigDbContext`. The unit-test project shows the same
conflict as warning MSB3277.

## Decision

The integration-test project pins `Microsoft.EntityFrameworkCore` and
`Microsoft.EntityFrameworkCore.Relational` at 10.0.11 to match the API project.

## Consequences

Two places state an EF Core version. Central package management replaces both with one entry
and removes the warning in the unit-test project as well.

## Sources

- Central package management: https://learn.microsoft.com/nuget/consume-packages/central-package-management
