namespace StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.DiscussionService.Domain.Enums;
public class DiscussionThreadParameters : QueryStringParameters
{

    public Guid? TargetId { get; set; }
    public TargetType? TargetType { get; set; }
    public Guid? CreatedById { get; set; }
    public bool? IsClosed { get; set; }
    public string? SearchText { get; set; }
    public int? MinCommentCount { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public DiscussionThreadParameters()
    {
        SortBy = "createdAt";
        SortDescending = true; 
    }
}