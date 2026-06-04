namespace StudentPortal.Enrollment.Domain.Entities;

public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
