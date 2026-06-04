namespace StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
public class ModuleUpdateDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
}