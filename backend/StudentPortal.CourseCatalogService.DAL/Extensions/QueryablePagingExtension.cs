namespace StudentPortal.CourseCatalogService.DAL.Extensions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using Microsoft.EntityFrameworkCore;

public static class QueryablePagingExtensions
{
        public static async Task<PagedList<T>> ToPagedListAsync<T, TParams>(
            this IQueryable<T> query,
            TParams parameters,
            CancellationToken cancellationToken = default)
            where TParams : QueryStringParameters
        {
            var page = parameters.Page;
            var pageSize = parameters.PageSize;

            var count = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<T>(items, count, page, pageSize);
        }
}