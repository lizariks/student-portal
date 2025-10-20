namespace StudentPortal.DiscussionService.Domain.Parameters;

public class CommentParameters : QueryStringParameters
{
    public Guid? ThreadId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public bool? IsResolved { get; set; }
    public string? SearchKeyword { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public CommentParameters()
    {
        SortBy = "createdAt";
    }
}