using AuthService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using DbContainer dbContext =
            scope.ServiceProvider.GetRequiredService<DbContainer>();

        dbContext.Database.Migrate();
    }
}