namespace StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
public class ModuleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int CourseId { get; set; }
    public ICollection<LessonDto> Lessons { get; set; } = new List<LessonDto>();
}