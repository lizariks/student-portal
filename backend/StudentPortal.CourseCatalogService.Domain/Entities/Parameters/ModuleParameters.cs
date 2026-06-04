namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class ModuleParameters : QueryStringParameters
{
    public int? CourseId { get; set; }
    public string? Title { get; set; }
    public string? OrderBy { get; set; } = "Order"; 
}