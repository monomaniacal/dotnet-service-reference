# 0004. Request contracts stay in the API project

Status: accepted, 2026-09-08

## Context

Request validation uses ASP.NET Core 10's built-in Minimal API validation, enabled with
`AddValidation()`. It is backed by a source generator that discovers validatable types only in
the assembly where it is invoked.

## Decision

The request and response records in `Contracts/` live in `ConfigService.Api`. They are not
moved to a shared or client project.

## Consequences

Moving them would silently disable validation with no build error and no runtime warning. A
client library, if one is ever needed, gets its own copies or a generated client from the
OpenAPI document.

## Sources

- Minimal API validation: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/validation
