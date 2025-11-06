using AuthService.Infrastructure.Database;
using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Database.Extensions;
using AuthService.Infrastructure.Database.Identity;
using AuthService.Infrastructure.Database.Transactions;
using AuthService.Infrastructure.DomainEvents;
using AuthService.Infrastructure.Segurity.Authentication;
using AuthService.Infrastructure.Segurity.Authorization;
using AuthService.Infrastructure.Segurity.Jwt;
using AuthService.Infrastructure.Segurity.Services;
using AuthService.Infrastructure.Time;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using UserModel = AuthService.Infrastructure.Database.Entities.User;
using TenantModel = AuthService.Infrastructure.Database.Entities.Tenant;

namespace AuthService.Infrastructure;

public static partial class DependencyInjection
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
            .AddSecurity(configuration)
            .AddAuthorizationInternal()
            .AddAutoMapperInfrastructure()
            .AddRepositories();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }
    
    private static IServiceCollection AddAutoMapperInfrastructure(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<DbContainer>(options => options
            .UseSqlServer(connectionString, opt =>
                opt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default)));

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrEmpty(connectionString))
            return services;

        services
            .AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration config)
    {
        services.AddJwtAuthentication(config);
        services.AddHttpContextAccessor();
        
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        
        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

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

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMapper mapper = serviceProvider.GetRequiredService<IMapper>();

        services.AddRepository<UserModel, Domain.Entities.User>();

        services.AddRepository<TenantModel, Domain.Entities.Tenant>();
        
        return services;
    }
}