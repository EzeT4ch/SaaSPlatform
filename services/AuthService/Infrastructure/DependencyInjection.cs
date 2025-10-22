using AuthService.Infrastructure.Database;
using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Database.Identity;
using AuthService.Infrastructure.Database.Transactions;
using AuthService.Infrastructure.DomainEvents;
using AuthService.Infrastructure.Segurity.Jwt;
using AuthService.Infrastructure.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddIdentity()
            .AddHealthChecks(configuration)
            .AddUnitOfWork()
            .AddSecurity(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<DbContainer>(options => options
            .UseSqlServer(connectionString, opt =>
                opt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default)));

        services.AddIdentity();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrEmpty(connectionString))
            return services;

        services
            .AddHealthChecks()
            .AddSqlServer(connectionString);

        return services;
    }

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration config)
    {
        services.AddJwtAuthentication(config);
        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddUserStore<ApplicationUserStore>()
            .AddRoleStore<RoleStore<Role, DbContainer, Guid>>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}