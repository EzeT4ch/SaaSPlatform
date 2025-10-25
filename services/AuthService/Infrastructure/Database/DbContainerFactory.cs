using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Database;

public class DbContainerFactory : IDesignTimeDbContextFactory<DbContainer>
{
    public DbContainer CreateDbContext(string[] args)
    {
        // Build configuration to read appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Web.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        DbContextOptionsBuilder<DbContainer> optionsBuilder = new DbContextOptionsBuilder<DbContainer>();
        string? connectionString = configuration.GetConnectionString("Database");
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new DbContainer(optionsBuilder.Options);
    }
}