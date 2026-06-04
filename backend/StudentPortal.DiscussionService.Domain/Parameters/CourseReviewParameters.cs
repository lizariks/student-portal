namespace StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.DiscussionService.Domain.Enums;
public class CourseReviewParameters : QueryStringParameters
{
    public Guid? TargetId { get; set; }
    public TargetType? TargetType { get; set; }
    public Guid? ReviewerId { get; set; }
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public string? SearchKeyword { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public CourseReviewParameters()
    {
        SortBy = "createdAt";
        SortDescending = true; 
    }
}