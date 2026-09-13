namespace ConfigService.Api.Contracts;

public record ApplicationDetailResponse(
    Guid Id, string Name, string? Comments, DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<Guid> ConfigurationIds);
