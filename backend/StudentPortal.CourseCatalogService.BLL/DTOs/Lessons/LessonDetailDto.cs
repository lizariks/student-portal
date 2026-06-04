namespace StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;
public class LessonDetailDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public int Order { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public ICollection<MaterialDto> Materials { get; set; } = new List<MaterialDto>();
}