using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PersonalDetailsApi.Contracts.Responses;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Common.Extensions;

/// <summary>Generic LINQ helpers that work for any entity type.</summary>
public static class QueryableExtensions
{
    public static IOrderedQueryable<T> OrderBy<T, TKey>(
        this IQueryable<T> source, Expression<Func<T, TKey>> keySelector, SortDirection direction) =>
        direction == SortDirection.Desc ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);

    public static IOrderedQueryable<T> ThenBy<T, TKey>(
        this IOrderedQueryable<T> source, Expression<Func<T, TKey>> keySelector, SortDirection direction) =>
        direction == SortDirection.Desc ? source.ThenByDescending(keySelector) : source.ThenBy(keySelector);

    /// <summary>Runs a COUNT and a page query, returning a <see cref="PagedResult{T}"/>.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, int page, int pageSize, CancellationToken ct = default)
    {
        var total = await source.CountAsync(ct);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, page, pageSize, total);
    }
}
