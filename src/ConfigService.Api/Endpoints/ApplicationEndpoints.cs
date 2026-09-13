using ConfigService.Api.Contracts;
using ConfigService.Api.Entities;
using ConfigService.Api.Infrastructure;
using ConfigService.Api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ConfigService.Api.Endpoints;

public static class ApplicationEndpoints
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/applications");

        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapGet("/", GetAllAsync);

        return app;
    }

    internal static async Task<Results<Created<ApplicationResponse>, ProblemHttpResult>> CreateAsync(
        CreateApplicationRequest request,
        IApplicationRepository repository,
        CancellationToken ct)
    {
        var application = new Application
        {
            Name = request.Name,
            Comments = request.Comments,
        };

        try
        {
            await repository.AddAsync(application, ct);
        }
        catch (DuplicateResourceException ex)
        {
            return TypedResults.Problem(
                ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
        }

        var response = ToResponse(application);
        return TypedResults.Created($"/api/v1/applications/{application.Id}", response);
    }

    internal static async Task<Results<Ok<ApplicationResponse>, NotFound, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateApplicationRequest request,
        IApplicationRepository repository,
        CancellationToken ct)
    {
        var application = await repository.GetByIdAsync(id, ct);
        if (application is null)
        {
            return TypedResults.NotFound();
        }

        application.Name = request.Name;
        application.Comments = request.Comments;

        try
        {
            await repository.UpdateAsync(application, ct);
        }
        catch (DuplicateResourceException ex)
        {
            return TypedResults.Problem(
                ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
        }

        return TypedResults.Ok(ToResponse(application));
    }

    internal static async Task<Results<Ok<ApplicationDetailResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IApplicationRepository repository,
        CancellationToken ct)
    {
        var result = await repository.GetByIdWithConfigurationIdsAsync(id, ct);
        if (result is null)
        {
            return TypedResults.NotFound();
        }

        var response = new ApplicationDetailResponse(
            result.Application.Id,
            result.Application.Name,
            result.Application.Comments,
            result.Application.CreatedAt,
            result.Application.UpdatedAt,
            result.ConfigurationIds);

        return TypedResults.Ok(response);
    }

    internal static async Task<Ok<IEnumerable<ApplicationResponse>>> GetAllAsync(
        IApplicationRepository repository,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return TypedResults.Ok(Enumerable.Empty<ApplicationResponse>());
        }

        var applications = await repository.GetAllAsync(ct);
        return TypedResults.Ok(applications.Select(ToResponse));
    }

    private static ApplicationResponse ToResponse(Application application) =>
        new(
            application.Id,
            application.Name,
            application.Comments,
            application.CreatedAt,
            application.UpdatedAt);
}
