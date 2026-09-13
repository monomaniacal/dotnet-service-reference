using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfigService.Api.Contracts;

namespace ConfigService.Api.IntegrationTests;

[Collection(ConfigServiceCollection.Name)]
public class JsonbRoundTripTests(PostgresFixture fixture)
{
    private const string ConfigJson =
        """
        {
          "stringValue": "hello world",
          "numberValue": 42.5,
          "boolValue": true,
          "nullValue": null,
          "arrayValue": [1, "two", false, null, { "nested": true }],
          "nested": {
            "level2": {
              "level3": {
                "deep": "value",
                "list": [1, 2, 3]
              }
            }
          }
        }
        """;

    [Fact]
    public async Task PostThenGet_DeeplyNestedConfig_RoundTripsStructurallyIdentical()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();

        var appResponse = await client.PostAsJsonAsync(
            "/api/v1/applications",
            new CreateApplicationRequest($"jsonb-roundtrip-app-{Guid.NewGuid()}", null),
            ct);
        appResponse.EnsureSuccessStatusCode();
        var application = (await appResponse.Content.ReadFromJsonAsync<ApplicationResponse>(ct))!;

        var configElement = JsonDocument.Parse(ConfigJson).RootElement;
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/configurations",
            new CreateConfigurationRequest(
                application.Id, $"jsonb-roundtrip-cfg-{Guid.NewGuid()}", "roundtrip test", configElement),
            ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        var getResponse = await client.GetAsync($"/api/v1/configurations/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = (await getResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        var expected = JsonNode.Parse(ConfigJson);
        var actual = JsonNode.Parse(fetched.Config.GetRawText());
        Assert.True(
            JsonNode.DeepEquals(expected, actual),
            $"Expected:\n{expected?.ToJsonString()}\nActual:\n{actual?.ToJsonString()}");
    }

    [Fact]
    public async Task PutConfiguration_WithIdenticalValues_DoesNotChangeUpdatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();

        var appResponse = await client.PostAsJsonAsync(
            "/api/v1/applications",
            new CreateApplicationRequest($"noop-put-app-{Guid.NewGuid()}", null),
            ct);
        appResponse.EnsureSuccessStatusCode();
        var application = (await appResponse.Content.ReadFromJsonAsync<ApplicationResponse>(ct))!;

        var configElement = JsonDocument.Parse("""{"k":"v","nested":{"n":1}}""").RootElement;
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/configurations",
            new CreateConfigurationRequest(
                application.Id, $"noop-put-cfg-{Guid.NewGuid()}", "no-op put test", configElement),
            ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        var getBeforeResponse = await client.GetAsync($"/api/v1/configurations/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.OK, getBeforeResponse.StatusCode);
        var before = (await getBeforeResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        await Task.Delay(50, ct);

        var putResponse = await client.PutAsJsonAsync(
            $"/api/v1/configurations/{created.Id}",
            new UpdateConfigurationRequest(before.Name, before.Comments, configElement),
            ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var afterPut = (await putResponse.Content.ReadFromJsonAsync<ConfigurationResponse>(ct))!;

        Assert.Equal(before.UpdatedAt, afterPut.UpdatedAt);
    }
}
