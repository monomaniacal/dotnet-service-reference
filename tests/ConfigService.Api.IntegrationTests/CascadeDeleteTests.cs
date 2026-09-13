using System.Net.Http.Json;
using System.Text.Json;
using ConfigService.Api.Contracts;
using ConfigService.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigService.Api.IntegrationTests;

[Collection(ConfigServiceCollection.Name)]
public class CascadeDeleteTests(PostgresFixture fixture)
{
    [Fact]
    public async Task DeletingApplicationRowDirectly_CascadesToItsConfigurations()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();

        var appResponse = await client.PostAsJsonAsync(
            "/api/v1/applications", new CreateApplicationRequest($"cascade-app-{Guid.NewGuid()}", null), ct);
        appResponse.EnsureSuccessStatusCode();
        var application = (await appResponse.Content.ReadFromJsonAsync<ApplicationResponse>(ct))!;

        var config = JsonDocument.Parse("""{"k":"v"}""").RootElement;
        var configResponse = await client.PostAsJsonAsync(
            "/api/v1/configurations",
            new CreateConfigurationRequest(application.Id, $"cascade-cfg-{Guid.NewGuid()}", null, config),
            ct);
        configResponse.EnsureSuccessStatusCode();
        var configuration = (await configResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
            var applicationEntity = await dbContext.Applications.FindAsync([application.Id], ct);
            Assert.NotNull(applicationEntity);
            dbContext.Applications.Remove(applicationEntity!);
            await dbContext.SaveChangesAsync(ct);
        }

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
            Assert.False(await dbContext.Applications.AnyAsync(a => a.Id == application.Id, ct));
            Assert.False(await dbContext.Configurations.AnyAsync(c => c.Id == configuration.Id, ct));
        }
    }
}
