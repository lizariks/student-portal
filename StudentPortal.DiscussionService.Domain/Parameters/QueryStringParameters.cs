namespace StudentPortal.DiscussionService.Domain.Parameters;

public abstract class QueryStringParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public string? OrderBy { get; set; } = "Id";
}