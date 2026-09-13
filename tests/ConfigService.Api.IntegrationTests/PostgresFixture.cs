using Testcontainers.PostgreSql;

namespace ConfigService.Api.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6-alpine").Build();

    public ConfigServiceFactory Factory { get; private set; } = null!;

    public HttpClient CreateClient() => Factory.CreateClient();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        Factory = new ConfigServiceFactory(_container.GetConnectionString());

        await Factory.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ConfigServiceCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "ConfigService shared Postgres collection";
}
