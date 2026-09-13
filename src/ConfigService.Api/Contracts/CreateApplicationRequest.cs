using System.ComponentModel.DataAnnotations;

namespace ConfigService.Api.Contracts;

public record CreateApplicationRequest(
    [Required, StringLength(256, MinimumLength = 1)] string Name,
    [StringLength(1024)] string? Comments);
