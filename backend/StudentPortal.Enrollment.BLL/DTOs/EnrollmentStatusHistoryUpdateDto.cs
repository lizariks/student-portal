namespace StudentPortal.Enrollment.BLL.DTOs;

public class EnrollmentStatusHistoryUpdateDto
{
    public int EnrollmentId { get; set; }
    public string NewStatus { get; set; } = null!;
}