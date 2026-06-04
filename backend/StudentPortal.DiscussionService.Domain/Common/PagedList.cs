namespace StudentPortal.DiscussionService.Domain.Common;
using MongoDB.Driver;
public class PagedList<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedList<T> Create(List<T> items, int count, int pageNumber, int pageSize)
    {
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
    
    public static async Task<PagedList<T>> ToPagedListAsync(
        IFindFluent<T, T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountDocumentsAsync(cancellationToken: cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, (int)count, pageNumber, pageSize);
    }
}
