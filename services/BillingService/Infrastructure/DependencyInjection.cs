using BillingService.Application.Abstractions.Data;
using BillingService.Infrastructure;
using BillingService.Infrastructure.Database;
using BillingService.Infrastructure.DomainEvents;
using BillingService.Infrastructure.Time;
using BillingService.Infrastructure.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace BillingService.Infrastructure;

public static class DependencyInjection
{
     public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<DbContainer>(
            options => options
                .UseSqlServer(connectionString, opt =>
                    opt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<DbContainer>());

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks();

        return services;
    }

   
}