# 0003. Integration tests run in a `Testing` environment against Testcontainers

Status: accepted, 2026-09-09

## Context

Integration tests exist only for what needs a real database: `jsonb` round-tripping, the
composite unique index, the cascade delete, and migrations applying to an empty database.
Migrations apply automatically only in the Development environment.

## Decision

- `ConfigServiceFactory` hosts the app with the environment set to `Testing`, so the
  Development-only auto-migrate never runs. Migrations are applied explicitly by the fixture,
  which makes the migration test a real test of the migrations rather than a side effect of
  startup.
- The connection string is injected after the app's own configuration sources so it always
  wins.
- Three test classes share one container through `PostgresFixture` to pay the startup cost
  once. `MigrationTests` deliberately uses its own fresh container so "applies cleanly to an
  empty database" is unambiguous.
- The cascade test deletes through `ConfigDbContext` directly because the API has no DELETE
  endpoint.

## Consequences

The suite starts two containers per run. Docker is required to run it; unit tests are not
affected.

## Sources

- Integration tests in ASP.NET Core: https://learn.microsoft.com/aspnet/core/test/integration-tests
- Testing against your production database system: https://learn.microsoft.com/ef/core/testing/testing-with-the-database
