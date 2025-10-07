namespace StudentPortal.CourseCatalogService.BLL.DTOs.Courses;

public class CourseUpdateDto
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public int? InstructorId { get; set; }
}