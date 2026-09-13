# OpenAPI document, generated at build time

## What

`Microsoft.Extensions.ApiDescription.Server` runs the app's entry point during `dotnet build`
and writes the OpenAPI document to `docs/openapi/v1.json`. The file is committed. CI fails if a
build produces a different document than the one committed. `Program.cs` skips persistence
registration when the entry assembly is the generator, so a build needs no database
configuration.

## Why

The API contract becomes a reviewable file: a pull request that changes a response shape
shows the change in the diff. The committed document is the input for the Tier 1 breaking-
change check and the DAST scan, so those tools never need a running service to know the
contract.

## Source

- Generate OpenAPI documents at build time: https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi#generate-openapi-documents-at-build-time
- Customising runtime behaviour during generation: the same page, "Customize runtime behavior during build-time document generation"

## Enforced by

The "OpenAPI document is up to date" step of `tier0-build`.

## Opt out

Remove `OpenApiGenerateDocumentsOnBuild` from the API project and the drift step from CI. The
runtime `/openapi/v1.json` endpoint keeps working in Development.

## Go deeper

Tier 1 item 23 adds oasdiff to fail pull requests on breaking changes. Endpoint metadata
(`WithName`, `WithSummary`, `WithTags`) enriches the document when a consumer needs it.
