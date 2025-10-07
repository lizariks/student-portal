namespace StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;

public class LessonUpdateDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int? Order { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
}