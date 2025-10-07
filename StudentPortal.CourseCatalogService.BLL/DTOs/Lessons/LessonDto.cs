namespace StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;

public class LessonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Order { get; set; }
}