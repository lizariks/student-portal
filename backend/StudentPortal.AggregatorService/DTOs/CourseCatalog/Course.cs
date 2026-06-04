namespace StudentPortal.AggregatorService.DTOs.CourseCatalog;

public class CourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? InstructorId { get; set; } 

    public ICollection<ModuleDto> Modules { get; set; } = new List<ModuleDto>();
}