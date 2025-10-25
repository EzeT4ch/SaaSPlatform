using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BillingService.Infrastructure.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    ///     Aplica múltiples Includes (y opcionalmente ThenIncludes) usando expresiones lambda.
    /// </summary>
    public static IQueryable<T> ApplyIncludes<T>(
        this IQueryable<T> query,
        params Expression<Func<T, object>>[]? includes) where T : class
    {
        if (includes is null || includes.Length == 0)
        {
            return query;
        }

        foreach (Expression<Func<T, object>> include in includes)
            query = query.Include(include);

        return query;
    }

    /// <summary>
    ///     Permite aplicar includes de forma fluida usando un constructor lambda
    /// </summary>
    public static IQueryable<T> ApplyIncludes<T>(
        this IQueryable<T> query,
        Func<IQueryable<T>, IQueryable<T>>? includeBuilder) where T : class
    {
        return includeBuilder?.Invoke(query) ?? query;
    }
}