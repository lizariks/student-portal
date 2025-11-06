namespace StudentPortal.AggregatorService.DTOs.CourseCatalog;

public class LessonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Order { get; set; } 
    public TimeSpan? EstimatedDuration { get; set; } 
}