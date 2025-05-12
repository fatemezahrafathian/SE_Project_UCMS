using Microsoft.EntityFrameworkCore;
using UCMS.DTOs;

namespace UCMS.Extensions;

public static class QueryableExtensions
{
    public static async Task<Page<T>> PaginateAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Page<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}