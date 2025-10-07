namespace StudentPortal.CourseCatalogService.BLL.DTOs.Materials;

public class MaterialUpdateDto
{
    public string Title { get; set; } = null!;
    public string? Url { get; set; }
    public string Type { get; set; } = null!;
    public int Order { get; set; }
}