# 0001. A no-op PUT leaves `updated_at` unchanged

Status: accepted, 2026-09-08

## Context

`created_at` and `updated_at` are maintained centrally by overriding `SaveChangesAsync`.
`updated_at` must move only when EF Core reports the entity as `Modified`. A PUT that submits
values identical to what is stored must leave the row untouched.

Two things would silently break that:

- Calling `DbSet.Update(entity)` in the repositories marks every scalar property modified
  regardless of whether it changed.
- `Configuration.Config` is a `JsonDocument`. EF Core compares reference types by reference,
  and the endpoint assigns a freshly parsed document on every PUT, so byte-identical content
  looks like a change. PostgreSQL `jsonb` also does not preserve object key order, so a
  document read back from the database can differ textually from the incoming one while being
  semantically identical.

## Decision

- Repositories never call `Update()`. Every caller loads the entity through the same
  `DbContext` first, so change tracking detects real modifications on its own.
- `Config` uses a content-based `ValueComparer` that canonicalises documents (object keys sorted
  at every level, array order preserved) before comparing and hashing.
- The integration test for this behaviour compares two database round-trips rather than the
  POST response and a later GET, because PostgreSQL `timestamptz` has microsecond precision
  and .NET `DateTime` has 100-nanosecond ticks.

## Consequences

Callers must mutate tracked entities; a detached entity passed to a repository is not saved.
The canonicaliser runs on every change-tracking comparison of `Config`, which is cheap at the
sizes this service stores.

## Sources

- EF Core value comparers: https://learn.microsoft.com/ef/core/modeling/value-comparers
- EF Core change tracking: https://learn.microsoft.com/ef/core/change-tracking/
- Npgsql JSON mapping: https://www.npgsql.org/efcore/mapping/json.html
