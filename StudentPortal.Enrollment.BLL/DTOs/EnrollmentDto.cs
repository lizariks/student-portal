namespace StudentPortal.Enrollment.BLL.DTOs;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = null!;
}