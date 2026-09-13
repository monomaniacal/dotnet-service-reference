# Configuration and secrets

## What

Settings bind to typed options classes in `src/ConfigService.Api/Options/` with
`AddOptions<T>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`. The
connection string lives in user secrets locally and in the `Database__ConnectionString`
environment variable elsewhere. `docs/configuration.md` lists every setting. No code reads
`Environment.GetEnvironmentVariable`; the configuration pipeline is the only path.

## Why

A missing or malformed setting fails at startup with the setting's name, not minutes later
with a connection error. Typed options give one place to see what the service needs. Keeping
the connection string out of committed files means the pattern for a real secret is already
in place when one arrives, and environment variables stay what they are meant to be: the
deployment-time override.

## Source

- Options pattern: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
- Options validation: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options#options-validation
- Safe storage of app secrets in development: https://learn.microsoft.com/aspnet/core/security/app-secrets
- Configuration in ASP.NET Core: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/

## Enforced by

`ValidateOnStart` at host start. `StartupValidationTests` proves a missing connection string
stops the host. Tier 2 architecture tests will forbid `Environment.GetEnvironmentVariable`
outside `Program.cs`.

## Opt out

Put the compose credentials back in `appsettings.Development.json`. You lose the demonstration
of the secrets path, not security: those credentials only open the local container defined in
`docker-compose.yml`.

## Go deeper

Tier 1 item 25 adds a readiness endpoint that checks the database. For deployed environments,
a secret store (Azure Key Vault, AWS Secrets Manager) is a configuration provider that slots
into the same pipeline; add it when a deploy target exists.
