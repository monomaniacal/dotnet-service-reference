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

public class ApplicationEndpointsTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedWithResponse()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var request = new CreateApplicationRequest("payments-service", "Handles payments");

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var assignedId = Guid.CreateVersion7();
        repository
            .AddAsync(Arg.Any<Application>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo =>
            {
                var application = callInfo.Arg<Application>();
                application.Id = assignedId;
                application.CreatedAt = createdAt;
                application.UpdatedAt = createdAt;
            });

        var result = await ApplicationEndpoints.CreateAsync(request, repository, CancellationToken.None);

        var created = Assert.IsType<Created<ApplicationResponse>>(result.Result);
        Assert.Equal($"/api/v1/applications/{assignedId}", created.Location);
        Assert.NotNull(created.Value);
        Assert.Equal(assignedId, created.Value!.Id);
        Assert.Equal("payments-service", created.Value.Name);
        Assert.Equal("Handles payments", created.Value.Comments);
        Assert.Equal(createdAt, created.Value.CreatedAt);
        Assert.Equal(createdAt, created.Value.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_WhenAddAsyncThrowsDuplicateResourceException_ReturnsConflict()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var request = new CreateApplicationRequest("payments-service", null);

        repository
            .AddAsync(Arg.Any<Application>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateResourceException(
                "An application named 'payments-service' already exists."));

        var result = await ApplicationEndpoints.CreateAsync(request, repository, CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("Conflict", problem.ProblemDetails.Title);
        Assert.Equal(
            "An application named 'payments-service' already exists.", problem.ProblemDetails.Detail);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsOkWithUpdatedResponse()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var id = Guid.CreateVersion7();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new Application
        {
            Id = id,
            Name = "old-name",
            Comments = "old comments",
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        repository
            .UpdateAsync(Arg.Any<Application>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo =>
            {
                callInfo.Arg<Application>().UpdatedAt =
                    new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            });

        var request = new UpdateApplicationRequest("new-name", "new comments");
        var result = await ApplicationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        var ok = Assert.IsType<Ok<ApplicationResponse>>(result.Result);
        Assert.NotNull(ok.Value);
        Assert.Equal(id, ok.Value!.Id);
        Assert.Equal("new-name", ok.Value.Name);
        Assert.Equal("new comments", ok.Value.Comments);
        Assert.Equal(createdAt, ok.Value.CreatedAt);
        Assert.Equal(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), ok.Value.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFound()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var id = Guid.CreateVersion7();
        repository
            .GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Application?)null);

        var request = new UpdateApplicationRequest("new-name", null);
        var result = await ApplicationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Application>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateAsyncThrowsDuplicateResourceException_ReturnsConflict()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var id = Guid.CreateVersion7();
        var existing = new Application
        {
            Id = id,
            Name = "old-name",
            Comments = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        repository
            .UpdateAsync(Arg.Any<Application>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateResourceException(
                "An application named 'new-name' already exists."));

        var request = new UpdateApplicationRequest("new-name", null);
        var result = await ApplicationEndpoints.UpdateAsync(id, request, repository, CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("Conflict", problem.ProblemDetails.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsOkWithDetailResponseAndConfigurationIds()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var id = Guid.CreateVersion7();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var application = new Application
        {
            Id = id,
            Name = "payments-service",
            Comments = "Handles payments",
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        var configurationIds = new List<Guid> { Guid.CreateVersion7(), Guid.CreateVersion7() };
        repository
            .GetByIdWithConfigurationIdsAsync(id, Arg.Any<CancellationToken>())
            .Returns(new ApplicationWithConfigurationIds(application, configurationIds));

        var result = await ApplicationEndpoints.GetByIdAsync(id, repository, CancellationToken.None);

        var ok = Assert.IsType<Ok<ApplicationDetailResponse>>(result.Result);
        Assert.NotNull(ok.Value);
        Assert.Equal(id, ok.Value!.Id);
        Assert.Equal("payments-service", ok.Value.Name);
        Assert.Equal("Handles payments", ok.Value.Comments);
        Assert.Equal(createdAt, ok.Value.CreatedAt);
        Assert.Equal(createdAt, ok.Value.UpdatedAt);
        Assert.Equal(configurationIds, ok.Value.ConfigurationIds);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var id = Guid.CreateVersion7();
        repository
            .GetByIdWithConfigurationIdsAsync(id, Arg.Any<CancellationToken>())
            .Returns((ApplicationWithConfigurationIds?)null);

        var result = await ApplicationEndpoints.GetByIdAsync(id, repository, CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkWithResponsesOmittingConfigurationIds()
    {
        var repository = Substitute.For<IApplicationRepository>();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var applications = new List<Application>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "app-one",
                Comments = null,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "app-two",
                Comments = "second app",
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            },
        };
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(applications);

        var result = await ApplicationEndpoints.GetAllAsync(repository, CancellationToken.None);

        Assert.IsType<Ok<IEnumerable<ApplicationResponse>>>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<ApplicationResponse>>(result.Value).ToList();
        Assert.Equal(2, responses.Count);
        Assert.Equal(applications[0].Id, responses[0].Id);
        Assert.Equal("app-one", responses[0].Name);
        Assert.Equal(applications[1].Id, responses[1].Id);
        Assert.Equal("app-two", responses[1].Name);

        Assert.DoesNotContain(
            typeof(ApplicationResponse).GetProperties(),
            property => property.Name == "ConfigurationIds");
    }
}
