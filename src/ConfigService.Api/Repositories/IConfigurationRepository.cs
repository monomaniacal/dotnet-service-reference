using ConfigService.Api.Entities;

namespace ConfigService.Api.Repositories;

public interface IConfigurationRepository
{
    Task<Configuration?> GetByIdAsync(Guid id, CancellationToken ct);

    Task AddAsync(Configuration configuration, CancellationToken ct);

    Task UpdateAsync(Configuration configuration, CancellationToken ct);
}
