namespace ConfigService.Api.Entities;

public sealed class Application : IAuditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Configuration> Configurations { get; set; } = [];
}
