namespace StudentPortal.AggregatorService.DTOs.CourseCatalog;

public class ModuleDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public int Order { get; set; } 

    public ICollection<LessonDto> Lessons { get; set; } = new List<LessonDto>();
}