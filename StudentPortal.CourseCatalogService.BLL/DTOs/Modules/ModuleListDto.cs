namespace StudentPortal.CourseCatalogService.BLL.DTOs.Modules;

public class ModuleListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Order { get; set; }
}