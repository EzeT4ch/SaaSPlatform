using Microsoft.Extensions.DependencyInjection;
using WmsService.Infrastructure.Database;
using WmsService.Infrastructure.Repositories;

namespace WmsService.Infrastructure.Extensions;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepository<TModel, TDomain>(
        this IServiceCollection services,
        Func<TModel, TDomain> toDomain,
        Func<TDomain, TModel> toModel)
        where TModel : class
        where TDomain : class
    {
        services.AddScoped<IRepository<TModel, TDomain>>(sp =>
        {

            DbContainer context = sp.GetRequiredService<DbContainer>();
            return new Repository<TModel, TDomain>(context, toDomain, toModel);
        });

        return services;
    }
}
