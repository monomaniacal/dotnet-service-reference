using ConfigService.Api.Entities;

namespace ConfigService.Api.Repositories;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<ApplicationWithConfigurationIds?> GetByIdWithConfigurationIdsAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken ct);

    Task AddAsync(Application application, CancellationToken ct);

    Task UpdateAsync(Application application, CancellationToken ct);
}
