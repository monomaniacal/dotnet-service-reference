using ConfigService.Api.Entities;

namespace ConfigService.Api.Repositories;

public sealed record ApplicationWithConfigurationIds(
    Application Application,
    IReadOnlyList<Guid> ConfigurationIds);
