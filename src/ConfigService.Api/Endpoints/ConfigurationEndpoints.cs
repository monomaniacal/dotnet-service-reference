using System.Text.Json;
using ConfigService.Api.Contracts;
using ConfigService.Api.Entities;
using ConfigService.Api.Infrastructure;
using ConfigService.Api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ConfigService.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/configurations");

        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);

        return app;
    }

    internal static async Task<Results<Created<ConfigurationResponse>, NotFound, ProblemHttpResult>> CreateAsync(
        CreateConfigurationRequest request,
        IConfigurationRepository repository,
        CancellationToken ct)
    {
        var configuration = new Configuration
        {
            ApplicationId = request.ApplicationId,
            Name = request.Name,
            Comments = request.Comments,
            Config = JsonDocument.Parse(request.Config.GetRawText()),
        };

        try
        {
            await repository.AddAsync(configuration, ct);
        }
        catch (ReferencedResourceNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (DuplicateResourceException ex)
        {
            return TypedResults.Problem(
                ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
        }

        var response = ToResponse(configuration);
        return TypedResults.Created($"/api/v1/configurations/{configuration.Id}", response);
    }

    internal static async Task<Results<Ok<ConfigurationResponse>, NotFound, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateConfigurationRequest request,
        IConfigurationRepository repository,
        CancellationToken ct)
    {
        var configuration = await repository.GetByIdAsync(id, ct);
        if (configuration is null)
        {
            return TypedResults.NotFound();
        }

        configuration.Name = request.Name;
        configuration.Comments = request.Comments;
        configuration.Config = JsonDocument.Parse(request.Config.GetRawText());

        try
        {
            await repository.UpdateAsync(configuration, ct);
        }
        catch (DuplicateResourceException ex)
        {
            return TypedResults.Problem(
                ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
        }

        return TypedResults.Ok(ToResponse(configuration));
    }

    internal static async Task<Results<Ok<ConfigurationResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IConfigurationRepository repository,
        CancellationToken ct)
    {
        var configuration = await repository.GetByIdAsync(id, ct);
        if (configuration is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(ToResponse(configuration));
    }

    private static ConfigurationResponse ToResponse(Configuration configuration) =>
        new(
            configuration.Id,
            configuration.ApplicationId,
            configuration.Name,
            configuration.Comments,
            configuration.Config.RootElement,
            configuration.CreatedAt,
            configuration.UpdatedAt);
}
