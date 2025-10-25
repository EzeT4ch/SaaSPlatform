using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Database;

public class DbContainerFactory : IDesignTimeDbContextFactory<DbContainer>
{
    public DbContainer CreateDbContext(string[] args)
    {
        // Construir configuración para leer appsettings.json
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