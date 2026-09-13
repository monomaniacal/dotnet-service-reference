namespace ConfigService.Api.Contracts;

public record ApplicationResponse(
    Guid Id, string Name, string? Comments, DateTime CreatedAt, DateTime UpdatedAt);
