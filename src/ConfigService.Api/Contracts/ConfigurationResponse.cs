using System.Text.Json;

namespace ConfigService.Api.Contracts;

public record ConfigurationResponse(
    Guid Id, Guid ApplicationId, string Name, string? Comments, JsonElement Config,
    DateTime CreatedAt, DateTime UpdatedAt);
