namespace StudentPortal.Enrollment.BLL.DTOs;

public class EnrollmentCreateDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string Status { get; set; } = null!;
}