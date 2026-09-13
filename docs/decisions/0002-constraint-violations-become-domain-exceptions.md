# 0002. Constraint violations become domain exceptions in repositories, status codes in endpoints

Status: accepted, 2026-09-08

## Context

Duplicate names and unknown foreign keys must be detected by the database, not by a query
before the write; a pre-check is a race condition dressed as validation. The API returns 409 for
a duplicate and 404 for a reference to a missing application, as RFC 9457 problem details, and
never leaks exception detail outside Development.

## Decision

- Repositories catch `DbUpdateException` whose inner `PostgresException` has SQLSTATE `23505`
  (unique violation) or `23503` (foreign key violation) and throw
  `DuplicateResourceException` or `ReferencedResourceNotFoundException` with a message that is
  safe to return to a client. Nothing above the repository layer knows about Npgsql.
- Endpoint handlers catch those two exceptions and return `TypedResults.Problem` with 409 or
  `TypedResults.NotFound`. This keeps the handler's return type honest for OpenAPI and unit
  tests.
- `GlobalExceptionHandler` maps the same two exceptions as a fallback for any call site that
  misses the local catch, and maps everything else to a bare 500. It injects
  `IProblemDetailsService` and delegates body writing to it. It logs 500s itself, because from
  .NET 10 the exception-handler middleware suppresses its own diagnostics for exceptions a
  handler reports as handled.

## Consequences

Adding a new constraint means adding a translation in the repository and a catch in the
handler. The fallback in the global handler means a missed catch degrades to a correct status
code rather than a 500.

## Sources

- Handle errors in ASP.NET Core APIs: https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api
- PostgreSQL error codes: https://www.postgresql.org/docs/current/errcodes-appendix.html
