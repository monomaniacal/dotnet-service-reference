using ConfigService.Api.Data;
using ConfigService.Api.Entities;
using ConfigService.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConfigService.Api.Repositories;

public sealed class ApplicationRepository(ConfigDbContext dbContext) : IApplicationRepository
{
    public Task<Application?> GetByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<ApplicationWithConfigurationIds?> GetByIdWithConfigurationIdsAsync(Guid id, CancellationToken ct)
    {
        var projection = await dbContext.Applications
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Comments,
                a.CreatedAt,
                a.UpdatedAt,
                ConfigurationIds = a.Configurations.Select(c => c.Id).ToList(),
            })
            .FirstOrDefaultAsync(ct);

        if (projection is null)
        {
            return null;
        }

        var application = new Application
        {
            Id = projection.Id,
            Name = projection.Name,
            Comments = projection.Comments,
            CreatedAt = projection.CreatedAt,
            UpdatedAt = projection.UpdatedAt,
        };

        return new ApplicationWithConfigurationIds(application, projection.ConfigurationIds);
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken ct) =>
        await dbContext.Applications.OrderBy(a => a.Name).ToListAsync(ct);

    public async Task AddAsync(Application application, CancellationToken ct)
    {
        application.Id = Guid.CreateVersion7();
        dbContext.Applications.Add(application);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateResourceException("An application with this name already exists.");
        }
    }

    public async Task UpdateAsync(Application application, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateResourceException("An application with this name already exists.");
        }
    }
}
