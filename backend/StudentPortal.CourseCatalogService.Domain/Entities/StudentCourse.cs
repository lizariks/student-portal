namespace StudentPortal.CourseCatalogService.Domain.Entities;

public class StudentCourse
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}