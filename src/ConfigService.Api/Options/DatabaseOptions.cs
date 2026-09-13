using System.ComponentModel.DataAnnotations;

namespace ConfigService.Api.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
