# API reference UI

## What

Scalar renders the OpenAPI document as an interactive reference at `/scalar`, mapped only in
the Development environment.

## Why

Exploring and calling the API without writing a client is the fastest way to learn what a
service does. The `.http` file covers scripted calls; the UI covers discovery.

## Source

- Microsoft's OpenAPI documentation names Scalar as the interactive UI option for .NET 9 and later and states UIs should only be enabled in development: https://learn.microsoft.com/aspnet/core/fundamentals/openapi/using-openapi-documents#use-scalar-for-interactive-api-documentation
- Scalar (third party, no Microsoft UI ships in the box): https://github.com/scalar/scalar

## Enforced by

`StartupValidationTests.Scalar_OutsideDevelopment_IsNotMapped` proves the route is absent
outside Development.

## Opt out

Remove the package reference and the `MapScalarApiReference()` call. The `.http` file and the
raw `/openapi/v1.json` endpoint remain.

## Go deeper

None planned. Swagger UI via `Swashbuckle.AspNetCore.SwaggerUi` is the alternative Microsoft
documents.
