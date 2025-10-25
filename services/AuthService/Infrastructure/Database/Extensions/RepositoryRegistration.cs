using AuthService.Infrastructure.Database.Repositories;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Database.Extensions;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepository<TModel, TDomain>(this IServiceCollection services)
        where TModel : class
        where TDomain : class
    {
        services.AddScoped<IRepository<TModel, TDomain>>(sp =>
        {
            DbContainer context = sp.GetRequiredService<DbContainer>();
            IMapper mapper = sp.GetRequiredService<IMapper>();

            return new Repository<TModel, TDomain>(
                context,
                toDomain: m => mapper.Map<TDomain>(m),
                toModel: d => mapper.Map<TModel>(d)
            );
        });

        return services;
    }
}