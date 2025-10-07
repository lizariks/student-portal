namespace StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;

public class LessonCreateDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public int Order { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
}