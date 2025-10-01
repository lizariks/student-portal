namespace StudentPortal.CourseCatalogService.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } 
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<StudentCourse> Enrollments { get; set; } = new List<StudentCourse>();

    public int? InstructorId { get; set; }
    public User Instructor { get; set; } = null!;
}