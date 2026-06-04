namespace StudentPortal.CourseCatalogService.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public int Order { get; set; } 
    public TimeSpan? EstimatedDuration { get; set; }

    public ICollection<Material> Materials { get; set; } = new List<Material>();
    public Module Module { get; set; }
}