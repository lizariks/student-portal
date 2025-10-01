namespace StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public UserRole Role { get; set; }

    public ICollection<Course> CoursesAsInstructor { get; set; } = new List<Course>();
    public ICollection<StudentCourse> Enrollments { get; set; } = new List<StudentCourse>();
}