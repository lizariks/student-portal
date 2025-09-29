namespace StudentPortal.Enrollment.BLL.DTOs;

public class StudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}