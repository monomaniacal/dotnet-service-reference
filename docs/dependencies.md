# Dependency register

One row per package in `Directory.Packages.props`. `scripts/CheckDependencyRegister.cs` fails
CI when a package has no row or a row has no package. The first column is the exact package id
in backticks.

| Package | Why it is here | What it replaces | Tier |
| --- | --- | --- | --- |
| `Microsoft.AspNetCore.Mvc.Testing` | Hosts the app in-process for integration tests | Hand-rolled test server | 0 |
| `Microsoft.AspNetCore.OpenApi` | Built-in OpenAPI document generation | Swashbuckle, NSwag | 0 |
| `Microsoft.EntityFrameworkCore` | Pinned so every project resolves the same EF Core version | Explicit per-project pins (ADR 0005) | 0 |
| `Microsoft.EntityFrameworkCore.Design` | Design-time support for `dotnet ef` migrations | Hand-written SQL migrations | 0 |
| `Microsoft.EntityFrameworkCore.Relational` | Pinned with the core package for the same reason | Explicit per-project pins (ADR 0005) | 0 |
| `Microsoft.Extensions.ApiDescription.Server` | Generates the OpenAPI document at build time | Committing a hand-fetched document | 0 |
| `Microsoft.NET.Test.Sdk` | Test host integration for `dotnet test` | none | 0 |
| `Microsoft.Testing.Extensions.CodeCoverage` | Cobertura coverage for the diff-coverage gate | coverlet | 0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core provider for PostgreSQL | raw Npgsql, Dapper | 0 |
| `NSubstitute` | Test doubles for repository interfaces in unit tests | Moq, hand-written fakes | 0 |
| `Scalar.AspNetCore` | Interactive API reference in Development (opt-out) | Swashbuckle.AspNetCore.SwaggerUi | 0 |
| `Testcontainers.PostgreSql` | Throwaway PostgreSQL for integration tests | EF InMemory provider, shared dev database | 0 |
| `xunit.runner.visualstudio` | Test discovery in IDEs | none | 0 |
| `xunit.v3` | Test framework on Microsoft.Testing.Platform | MSTest, NUnit | 0 |
