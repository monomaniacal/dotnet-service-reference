using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ConfigService.Api.Contracts;

public record UpdateConfigurationRequest(
    [Required, StringLength(256, MinimumLength = 1)] string Name,
    [StringLength(1024)] string? Comments,
    JsonElement Config) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Config.ValueKind != JsonValueKind.Object)
        {
            yield return new ValidationResult(
                "Config must be a JSON object.", [nameof(Config)]);
        }
    }
}
