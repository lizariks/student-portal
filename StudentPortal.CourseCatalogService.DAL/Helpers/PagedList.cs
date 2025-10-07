namespace StudentPortal.CourseCatalogService.DAL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

    public class PagedList<T> : List<T>
    {
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public PagedList(List<T> items, int totalCount, int page, int pageSize)
        {
            AddRange(items);
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public static async Task<PagedList<T>> ToPagedListAsync(
            IQueryable<T> source, int page, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagedList<T>(items, count, page, pageSize);
        }
    }
