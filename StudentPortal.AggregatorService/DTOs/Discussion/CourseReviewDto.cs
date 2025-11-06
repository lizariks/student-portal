namespace StudentPortal.AggregatorService.DTOs.Discussion;

public class CourseReviewDto
{
    public string TargetId { get; set; } = null!;
    
    public double AverageRating { get; set; } 
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; } 
    public int OneStarCount { get; set; } 
}