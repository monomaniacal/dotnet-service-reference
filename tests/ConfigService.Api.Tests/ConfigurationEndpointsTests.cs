using System.Text.Json;
using ConfigService.Api.Contracts;
using ConfigService.Api.Endpoints;
using ConfigService.Api.Entities;
using ConfigService.Api.Infrastructure;
using ConfigService.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ConfigService.Api.Tests;

public class ConfigurationEndpointsTests
{
    private static JsonElement ParseElement(string json) =>
        JsonDocument.Parse(json).RootElement;

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedWithResponseAndConfigRoundTrips()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var applicationId = Guid.CreateVersion7();
        var config = ParseElement(
            """
            {
              "nested": { "flag": true, "count": 3, "values": [1, 2, 3] },
              "topLevel": "value"
            }
            """);
        var request = new CreateConfigurationRequest(applicationId, "primary-config", "Main config", config);

        var assignedId = Guid.CreateVersion7();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        repository
            .AddAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo =>
            {
                var configuration = callInfo.Arg<Configuration>();
                configuration.Id = assignedId;
                configuration.CreatedAt = createdAt;
                configuration.UpdatedAt = createdAt;
            });

        var result = await ConfigurationEndpoints.CreateAsync(request, repository, CancellationToken.None);

        var created = Assert.IsType<Created<ConfigurationResponse>>(result.Result);
        Assert.Equal($"/api/v1/configurations/{assignedId}", created.Location);
        Assert.NotNull(created.Value);
        Assert.Equal(assignedId, created.Value!.Id);
        Assert.Equal(applicationId, created.Value.ApplicationId);
        Assert.Equal("primary-config", created.Value.Name);
        Assert.Equal("Main config", created.Value.Comments);
        Assert.Equal(createdAt, created.Value.CreatedAt);
        Assert.Equal(createdAt, created.Value.UpdatedAt);

        Assert.Equal(JsonValueKind.Object, created.Value.Config.ValueKind);
        var nested = created.Value.Config.GetProperty("nested");
        Assert.Equal(JsonValueKind.Object, nested.ValueKind);
        Assert.True(nested.GetProperty("flag").GetBoolean());
        Assert.Equal(3, nested.GetProperty("count").GetInt32());
        Assert.Equal([1, 2, 3], nested.GetProperty("values").EnumerateArray().Select(e => e.GetInt32()));
        Assert.Equal("value", created.Value.Config.GetProperty("topLevel").GetString());

        await repository.Received(1).AddAsync(
            Arg.Is<Configuration>(c =>
                c.Config.RootElement.GetProperty("nested").GetProperty("count").GetInt32() == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenAddAsyncThrowsReferencedResourceNotFoundException_ReturnsNotFound()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var request = new CreateConfigurationRequest(
            Guid.CreateVersion7(), "primary-config", null, ParseElement("{}"));

        repository
            .AddAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ReferencedResourceNotFoundException(
                "No application exists with the given application_id."));

        var result = await ConfigurationEndpoints.CreateAsync(request, repository, CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task CreateAsync_WhenAddAsyncThrowsDuplicateResourceException_ReturnsConflict()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var request = new CreateConfigurationRequest(
            Guid.CreateVersion7(), "primary-config", null, ParseElement("{}"));

        repository
            .AddAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateResourceException(
                "A configuration named 'primary-config' already exists for this application."));

        var result = await ConfigurationEndpoints.CreateAsync(request, repository, CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("Conflict", problem.ProblemDetails.Title);
        Assert.Equal(
            "A configuration named 'primary-config' already exists for this application.",
            problem.ProblemDetails.Detail);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsOkWithUpdatedResponse()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var id = Guid.CreateVersion7();
        var applicationId = Guid.CreateVersion7();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new Configuration
        {
            Id = id,
            ApplicationId = applicationId,
            Name = "old-name",
            Comments = "old comments",
            Config = JsonDocument.Parse("""{"old":true}"""),
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        var updatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        repository
            .UpdateAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => callInfo.Arg<Configuration>().UpdatedAt = updatedAt);

        var newConfig = ParseElement("""{"new":"value"}""");
        var request = new UpdateConfigurationRequest("new-name", "new comments", newConfig);
        var result = await ConfigurationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        var ok = Assert.IsType<Ok<ConfigurationResponse>>(result.Result);
        Assert.NotNull(ok.Value);
        Assert.Equal(id, ok.Value!.Id);
        Assert.Equal(applicationId, ok.Value.ApplicationId);
        Assert.Equal("new-name", ok.Value.Name);
        Assert.Equal("new comments", ok.Value.Comments);
        Assert.Equal("value", ok.Value.Config.GetProperty("new").GetString());
        Assert.Equal(createdAt, ok.Value.CreatedAt);
        Assert.Equal(updatedAt, ok.Value.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFound()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var id = Guid.CreateVersion7();
        repository
            .GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Configuration?)null);

        var request = new UpdateConfigurationRequest("new-name", null, ParseElement("{}"));
        var result = await ConfigurationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateAsyncThrowsDuplicateResourceException_ReturnsConflict()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var id = Guid.CreateVersion7();
        var existing = new Configuration
        {
            Id = id,
            ApplicationId = Guid.CreateVersion7(),
            Name = "old-name",
            Comments = null,
            Config = JsonDocument.Parse("{}"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        repository
            .UpdateAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateResourceException(
                "A configuration named 'new-name' already exists for this application."));

        var request = new UpdateConfigurationRequest("new-name", null, ParseElement("{}"));
        var result = await ConfigurationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("Conflict", problem.ProblemDetails.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsOkWithResponse()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var id = Guid.CreateVersion7();
        var applicationId = Guid.CreateVersion7();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var configuration = new Configuration
        {
            Id = id,
            ApplicationId = applicationId,
            Name = "primary-config",
            Comments = "Main config",
            Config = JsonDocument.Parse("""{"key":"value"}"""),
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(configuration);

        var result = await ConfigurationEndpoints.GetByIdAsync(id, repository, CancellationToken.None);

        var ok = Assert.IsType<Ok<ConfigurationResponse>>(result.Result);
        Assert.NotNull(ok.Value);
        Assert.Equal(id, ok.Value!.Id);
        Assert.Equal(applicationId, ok.Value.ApplicationId);
        Assert.Equal("primary-config", ok.Value.Name);
        Assert.Equal("Main config", ok.Value.Comments);
        Assert.Equal("value", ok.Value.Config.GetProperty("key").GetString());
        Assert.Equal(createdAt, ok.Value.CreatedAt);
        Assert.Equal(createdAt, ok.Value.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        var repository = Substitute.For<IConfigurationRepository>();
        var id = Guid.CreateVersion7();
        repository
            .GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Configuration?)null);

        var result = await ConfigurationEndpoints.GetByIdAsync(id, repository, CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }
}
