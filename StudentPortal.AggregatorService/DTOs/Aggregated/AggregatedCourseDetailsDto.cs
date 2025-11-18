namespace StudentPortal.AggregatorService.DTOs.Aggregated;

public class AggregatedCourseCourseDetailsDto 
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Code { get; set; }
    public int InstructorId { get; set; }
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
}