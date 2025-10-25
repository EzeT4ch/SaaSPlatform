using BillingService.Infrastructure.Database;
using BillingService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingService.Infrastructure.Extensions;

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