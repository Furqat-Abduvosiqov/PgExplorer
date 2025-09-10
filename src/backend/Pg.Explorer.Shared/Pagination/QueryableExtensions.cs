using System.Linq.Expressions;

namespace Pg.Explorer.Shared.Pagination;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;

    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string? sortBy, bool desc)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return source;
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(parameter, sortBy);
        var keySelector = Expression.Lambda(property, parameter);
        var methodName = desc ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2);
        var generic = method.MakeGenericMethod(typeof(T), property.Type);
        return (IQueryable<T>)generic.Invoke(null, new object[] { source, keySelector })!;
    }

    public static async Task<PageResult<T>> ToPageResultAsync<T>(this IQueryable<T> source, int page, int pageSize,
        CancellationToken ct = default)
    {
        var total = source.Count();
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PageResult<T>(items, page, pageSize, total);
    }
}