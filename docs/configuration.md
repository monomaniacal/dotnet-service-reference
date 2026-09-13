# Configuration reference

Every setting the service reads. Settings bind to typed options classes and are validated
when the host starts; a missing required setting stops startup with a message naming it.

Sources, in the order ASP.NET Core applies them (later wins): `appsettings.json`,
`appsettings.{Environment}.json`, user secrets (Development only), environment variables,
command-line arguments.

| Key | Environment variable | Options type | Required | Default | Notes |
| --- | --- | --- | --- | --- | --- |
| `Database:ConnectionString` | `Database__ConnectionString` | `DatabaseOptions` | yes | none | Npgsql connection string. Locally: `dotnet user-secrets set "Database:ConnectionString" "..." --project src/ConfigService.Api`. |
| `Logging:LogLevel:Default` | `Logging__LogLevel__Default` | framework | no | `Information` | Standard logging configuration. |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Logging__LogLevel__Microsoft.AspNetCore` | framework | no | `Warning` | |
| `AllowedHosts` | `AllowedHosts` | framework | no | `*` | Host filtering. |
| `ASPNETCORE_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | framework | no | `Production` | `Development` enables auto-migrate, the OpenAPI endpoint, and the API reference UI. |

Adding a setting: add a property to an options class (or a new class in `src/ConfigService.Api/Options/`),
bind it in `Program.cs` with `ValidateDataAnnotations().ValidateOnStart()`, and add a row here.
