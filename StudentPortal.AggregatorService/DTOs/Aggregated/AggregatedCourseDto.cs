namespace StudentPortal.AggregatorService.DTOs.Aggregated;

using StudentPortal.AggregatorService.DTOs.Discussion;
public class AggregatedCourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public int? InstructorId { get; set; }
    public DateTime CreatedAt { get; set; }

    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }

    public List<string> ModuleTitles { get; set; } = new(); 
    public int TotalLessonsCount { get; set; }
    public TimeSpan TotalEstimatedDuration { get; set; } 

    public ICollection<DiscussionThreadDto> DiscussionThreads { get; set; } = new List<DiscussionThreadDto>();
}