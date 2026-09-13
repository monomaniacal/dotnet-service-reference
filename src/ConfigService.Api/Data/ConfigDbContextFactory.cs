using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConfigService.Api.Data;

public sealed class ConfigDbContextFactory : IDesignTimeDbContextFactory<ConfigDbContext>
{
    public ConfigDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=configservice;Username=configservice;Password=configservice";

        var optionsBuilder = new DbContextOptionsBuilder<ConfigDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ConfigDbContext(optionsBuilder.Options, TimeProvider.System);
    }
}
