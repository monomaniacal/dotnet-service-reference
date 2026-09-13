using ConfigService.Api.Data;
using ConfigService.Api.Entities;
using ConfigService.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConfigService.Api.Repositories;

public sealed class ConfigurationRepository(ConfigDbContext dbContext) : IConfigurationRepository
{
    public Task<Configuration?> GetByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.Configurations.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Configuration configuration, CancellationToken ct)
    {
        configuration.Id = Guid.CreateVersion7();
        dbContext.Configurations.Add(configuration);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateResourceException(
                "A configuration with this name already exists for this application.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new ReferencedResourceNotFoundException(
                "The specified application does not exist.");
        }
    }

    public async Task UpdateAsync(Configuration configuration, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateResourceException(
                "A configuration with this name already exists for this application.");
        }
    }
}
