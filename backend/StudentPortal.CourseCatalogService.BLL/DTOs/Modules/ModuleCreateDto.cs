namespace StudentPortal.CourseCatalogService.BLL.DTOs.Modules;

public class ModuleCreateDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int CourseId { get; set; }
}