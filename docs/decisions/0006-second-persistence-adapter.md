# 0006. A second persistence adapter behind the repository interfaces

Status: proposed, deferred on 2026-09-13. Backlog item 38.

## Context

The service has one persistence implementation: EF Core over PostgreSQL. The endpoints depend
on `IApplicationRepository` and `IConfigurationRepository`, and the unit tests substitute those
with NSubstitute. Nothing today proves that the interfaces are honest ports rather than a thin
wrapper over EF Core with PostgreSQL assumptions leaking through.

A second implementation would prove that, and would give a way to run the service with no
Docker, which matters once the template package generates new services.

Two facts frame the options:

- Microsoft's EF Core testing guidance says the repository pattern is the only reliable place to
  stub or mock the data layer, recommends testing data-access code against the real database,
  and discourages faking EF Core with SQLite or the InMemory provider because provider
  behaviour diverges.
- Microsoft's unit-testing terminology: a stub supplies data, a mock is asserted against, and
  the classic literature calls a working alternative implementation a fake. A second adapter
  is a fake in that sense, not a mock or a stub, so it is a different tool from NSubstitute and
  does not replace it.

Whatever the adapter, it must honour every invariant the PostgreSQL one does: duplicate names
raise `DuplicateResourceException`, an unknown application id raises
`ReferencedResourceNotFoundException`, deleting an application removes its configurations,
timestamps come from `TimeProvider`, and a no-op PUT leaves `updated_at` alone (ADR 0001). A
contract test suite, one abstract class per interface run against every adapter, is what makes
the second adapter worth having. Database-only tests (ADR 0003) stay as they are.

## Options

### 1. In-memory adapter

Dictionary-backed implementations of both interfaces.

- For: cheapest to write and to keep correct. Proves the port with nothing else in the loop.
  Zero infrastructure. No file semantics to get wrong.
- Against: data does not survive a restart. Single process only.
- Microsoft source: the repository pattern section of
  https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy

### 2. JSON file adapter

The in-memory adapter plus persistence to a file through `System.Text.Json`.

- For: data survives restarts, and the file is readable, which suits demos.
- Against: needs a lock, atomic replace on write, and a corruption story. Single instance
  only. A second set of invariants (file I/O) to test on top of the contract suite.
- Microsoft source: none for the pattern. `System.Text.Json` is the serializer.

### 3. SQLite through the EF Core provider

Same EF Core repositories, second provider, chosen by configuration.

- For: a real relational database from a Microsoft-maintained provider, with no
  infrastructure. Unique indexes and cascades are enforced by the engine.
- Against: it does not prove the port, because EF Core is still shaping every query.
  `jsonb` does not exist in SQLite, so the model or the value converters diverge. Error
  translation is provider-specific (SQLite error codes, not SQLSTATE). EF migrations are
  provider-specific, so a second migration set is needed. Microsoft says provider divergence
  tends to become a problem even when it looks fine at first.
- Microsoft source: the SQLite section of
  https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy

### 4. EF Core InMemory provider

Rejected. Microsoft discourages it for testing and supports it only for legacy applications.

### 5. Third-party embedded stores

Rejected. Not Microsoft-first, and a dependency with no trigger.

## Leaning

Option 1 with the contract suite delivers the reason for doing this, an honest port, at the
lowest cost. Option 2 is an upgrade to option 1 with a clear trigger: demo data must survive a
restart. Option 3 is the answer to a different question, "do we need a second real database",
and should only be chosen if that question is asked.

## Decision

Deferred. The trigger to revisit is the template package landing (item 26) or a need to run
the service without Docker.
