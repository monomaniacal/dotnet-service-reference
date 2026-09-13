using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConfigService.Api.Contracts;

namespace ConfigService.Api.IntegrationTests;

[Collection(ConfigServiceCollection.Name)]
public class UniqueIndexTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SameConfigurationName_UnderDifferentApplications_BothSucceed()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();
        var configName = $"shared-name-{Guid.NewGuid()}";
        var config = JsonDocument.Parse("""{"k":"v"}""").RootElement;

        var app1 = await CreateApplicationAsync(client, ct);
        var app2 = await CreateApplicationAsync(client, ct);

        var response1 = await client.PostAsJsonAsync(
            "/api/v1/configurations", new CreateConfigurationRequest(app1.Id, configName, null, config), ct);
        var response2 = await client.PostAsJsonAsync(
            "/api/v1/configurations", new CreateConfigurationRequest(app2.Id, configName, null, config), ct);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }

    [Fact]
    public async Task SameConfigurationName_TwiceUnderSameApplication_SecondPostReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();
        var app = await CreateApplicationAsync(client, ct);
        var configName = $"dup-name-{Guid.NewGuid()}";
        var config = JsonDocument.Parse("""{"k":"v"}""").RootElement;

        var first = await client.PostAsJsonAsync(
            "/api/v1/configurations", new CreateConfigurationRequest(app.Id, configName, null, config), ct);
        var second = await client.PostAsJsonAsync(
            "/api/v1/configurations", new CreateConfigurationRequest(app.Id, configName, null, config), ct);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private static async Task<ApplicationResponse> CreateApplicationAsync(HttpClient client, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/applications", new CreateApplicationRequest($"unique-index-app-{Guid.NewGuid()}", null), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApplicationResponse>(ct))!;
    }
}
