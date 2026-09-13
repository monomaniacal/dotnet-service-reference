using System.Text.Json;

namespace ConfigService.Api.Entities;

public sealed class Configuration : IAuditable
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }
    public JsonDocument Config { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Application Application { get; set; } = null!;
}
