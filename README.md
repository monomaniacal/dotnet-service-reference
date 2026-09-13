# dotnet-service-reference

A reference .NET service showing engineering practices that are enforced by tooling and
explained in docs, and the source of a `dotnet new` template for new services.

- **Run or read the service:** continue below.
- **Adopt a practice:** `docs/` holds the design spec, the decision records, and, as they land,
  one page per practice with its rationale and Microsoft source.
- **Start a new service:** the template package is on the backlog; see the spec.

Design and backlog: `docs/specs/2026-09-13-reference-repo-design.md`.

---

# ConfigService.Api

A small ASP.NET Core 10 Minimal API for managing **applications** and their **configurations**
(arbitrary JSON blobs, scoped to an application) backed by PostgreSQL.

## Prerequisites

- **.NET 10 SDK**
- **Docker** (with Compose support — `docker compose`, not the old `docker-compose`)
- Repo-local tools (`dotnet-ef`, `husky`) come from the tool manifest. After cloning, run:

  ```bash
  dotnet tool restore
  ```

All commands below are run from `project/config-service/` (this directory) unless noted
otherwise.

## Starting the database

```bash
docker compose up -d
```

This starts a single `postgres:18.6-alpine` container (`configservice-postgres`) on port 5432,
with credentials/database name `configservice`/`configservice`/`configservice` (see
`docker-compose.yml` and `src/ConfigService.Api/appsettings.Development.json`). Data persists in
a named Docker volume across restarts. Run `docker compose down` to stop it, or `docker compose
down -v` to also delete the volume and start from a truly empty database next time.

Then tell the service how to reach it. The connection string is not committed; store it once
per machine in user secrets:

```bash
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=configservice;Username=configservice;Password=configservice" --project src/ConfigService.Api
```

Every setting the service reads is listed in `docs/configuration.md`.

## Running the service

```bash
dotnet run --project src/ConfigService.Api
```

By default (the `http` launch profile, see `src/ConfigService.Api/Properties/launchSettings.json`)
this listens on **`http://localhost:5033`** and runs with `ASPNETCORE_ENVIRONMENT=Development`.

**In Development, the app applies pending EF Core migrations automatically on startup** — see the
end of `Program.cs`: `if (app.Environment.IsDevelopment()) { ... Database.MigrateAsync(); }`. As
long as `docker compose up -d` has already brought up Postgres, you don't need to run any
migration command yourself for local development; just `dotnet run` and the schema is created/
updated for you.

**This auto-migrate behavior is Development-only.** No other environment ever migrates itself —
there is no startup flag, environment-variable switch, or HTTP endpoint that triggers it. For any
other environment (Staging, Production, etc.), an operator must apply migrations explicitly,
ahead of time, against that environment's own connection string:

```bash
dotnet ef database update --project src/ConfigService.Api --startup-project src/ConfigService.Api
```

`dotnet ef` builds the app's own host, so it reads the same configuration sources. Locally,
pass `-- --environment Development` so user secrets are loaded:

```bash
dotnet ef database update --project src/ConfigService.Api -- --environment Development
```

To target a different environment's database, set `Database__ConnectionString` in the
environment before running the command (or point `--connection` at the target string directly),
then run it from a machine that can reach that database. This is the *only* way non-Development
environments get their schema — if you skip it, the app will fail against an out-of-date/empty
database.

## Trying the API

In Development the service also serves an interactive API reference at
`http://localhost:5033/scalar`, generated from the same OpenAPI document.

Once the service is running, use `src/ConfigService.Api/ConfigService.Api.http` — open it in VS
Code with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
extension and use "Send Request" above each block. The requests are chained: creating an
application captures its `id` into a file variable that the following requests reuse, so you can
run them top to bottom. Equivalent `curl` calls work too, e.g.:

```bash
curl http://localhost:5033/health

curl -X POST http://localhost:5033/api/v1/applications \
  -H "Content-Type: application/json" \
  -d '{"name":"Sample Application","comments":"created via curl"}'
```

## Running the tests

Unit tests (no Docker required — pure handler-level tests against mocked repositories):

```bash
dotnet test tests/ConfigService.Api.Tests
```

Integration tests (**requires Docker running** — this suite spins up its own throwaway
Testcontainers Postgres instance, entirely separate from the `docker compose` one above, so it's
safe to run whether or not `docker compose up -d` has been done):

```bash
dotnet test tests/ConfigService.Api.IntegrationTests
```

Plain `dotnet test` is all you need for both — `global.json` opts the repo into the Microsoft.
Testing.Platform runner (`"test": { "runner": "Microsoft.Testing.Platform" }`), but that changes
how `dotnet test` behaves under the hood, not the command itself. No extra flags are required.

You can also build/test everything from the solution at once:

```bash
dotnet build ConfigService.sln
dotnet test ConfigService.sln
```

## A note on `Contracts/`

`src/ConfigService.Api/Contracts/` (the request/response records) **must stay inside the
`ConfigService.Api` project.** ASP.NET Core 10's built-in `AddValidation()` source generator only
discovers validatable types in the assembly it's invoked from — moving `Contracts/` out to a
separate project would silently stop all request validation (`[Required]`, `[StringLength]`, the
`Config`-must-be-a-JSON-object check, etc.) with no build error and no runtime warning. This is
also called out as a comment in `Contracts/README.md`.

## API surface

All resource endpoints are prefixed `/api/v1`; `/health` is a liveness check at the root (no DB
check). No auth, no pagination, no DELETE endpoints.

| Method | Path                          | Notes                                              |
| ------ | ----------------------------- | --------------------------------------------------- |
| GET    | `/health`                     | Liveness only                                        |
| POST   | `/api/v1/applications`        | 201 + created application                            |
| PUT    | `/api/v1/applications/{id}`   | Full replace; 404 if unknown                         |
| GET    | `/api/v1/applications/{id}`   | Includes related configuration ids                   |
| GET    | `/api/v1/applications`        | 200 + array (empty array if none), no configuration ids |
| POST   | `/api/v1/configurations`      | 404 if `applicationId` doesn't exist, 409 on duplicate name within the application |
| PUT    | `/api/v1/configurations/{id}` | Full replace (name/comments/config); 404 if unknown  |
| GET    | `/api/v1/configurations/{id}` |                                                       |

Duplicate `applications.name`, or a duplicate `configurations.name` within the same application,
returns `409 Conflict`. Validation failures return `400 Bad Request`. All error responses are RFC
9457 Problem Details.
