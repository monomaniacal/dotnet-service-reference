using System.Net;
using System.Net.Http.Json;
using ConfigService.Api.Contracts;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ConfigService.Api.IntegrationTests;

public sealed class MigrationTests : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6-alpine").Build();

    private ConfigServiceFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _factory = new ConfigServiceFactory(_container.GetConnectionString());
        await _factory.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Migrations_OnFreshDatabase_CreateExpectedSchemaAndSupportBasicCrud()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync(ct);

        var applicationsColumns = await GetColumnNamesAsync(connection, "applications", ct);
        Assert.Equal(
            new HashSet<string> { "id", "name", "comments", "created_at", "updated_at" },
            applicationsColumns);

        var configurationsColumns = await GetColumnNamesAsync(connection, "configurations", ct);
        Assert.Equal(
            new HashSet<string>
            {
                "id", "application_id", "name", "comments", "config", "created_at", "updated_at",
            },
            configurationsColumns);

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/applications", new CreateApplicationRequest($"migration-check-{Guid.NewGuid()}", null), ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ApplicationResponse>(ct);
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/v1/applications/{created!.Id}", ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    private static async Task<HashSet<string>> GetColumnNamesAsync(
        NpgsqlConnection connection, string tableName, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT column_name FROM information_schema.columns " +
            "WHERE table_schema = 'public' AND table_name = @tableName";
        command.Parameters.AddWithValue("tableName", tableName);

        var columns = new HashSet<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }
}
