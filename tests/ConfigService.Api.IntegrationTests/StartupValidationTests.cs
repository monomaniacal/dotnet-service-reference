using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ConfigService.Api.IntegrationTests;

public sealed class StartupValidationTests
{
    [Fact]
    public void CreateClient_WithoutConnectionString_ThrowsOptionsValidation()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        var exception = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("ConnectionString", exception.Message);
    }

    [Fact]
    public async Task Health_WithConnectionString_Returns200WithoutTouchingTheDatabase()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ConnectionString"] = "Host=unreachable;Database=x;Username=x;Password=x",
                    }));
            });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Scalar_OutsideDevelopment_IsNotMapped()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ConnectionString"] = "Host=unreachable;Database=x;Username=x;Password=x",
                    }));
            });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/scalar", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
