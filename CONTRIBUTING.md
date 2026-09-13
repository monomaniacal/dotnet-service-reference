# Contributing

## The loop

1. Branch from `main`.
2. Write the failing test. Run it and watch it fail:

   ```bash
   dotnet test tests/ConfigService.Api.Tests --filter-method "*YourTestName*"
   ```

   Or keep the loop running while you work:

   ```bash
   dotnet watch test --project tests/ConfigService.Api.Tests
   ```

3. Write the smallest change that makes it pass. Run it again.
4. Commit. The pre-commit hook checks formatting and secrets.
5. Open a pull request. Fill in every section of the template; "which test drove it" is not
   optional.

## Tests

- Unit tests (`tests/ConfigService.Api.Tests`) use NSubstitute for repository interfaces and
  need no database. They run in seconds.
- Integration tests (`tests/ConfigService.Api.IntegrationTests`) need Docker and cover only
  what needs the database (ADR 0003).
- Test names follow `Method_Scenario_Expectation`.

## Adding a dependency

Add the version to `Directory.Packages.props`, reference it by name in the project, and add a
row to `docs/dependencies.md` saying why it is here and what it replaces. CI fails without the
row.

## Adding a setting

Add a property to an options class, bind it with `ValidateOnStart()` in `Program.cs`, and add
a row to `docs/configuration.md`.

## Rationale

Not in code comments. A practice gets a page in `docs/practices/` (six parts: What, Why,
Source, Enforced by, Opt out, Go deeper). A decision gets an ADR in `docs/decisions/`.
