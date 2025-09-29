namespace StudentPortal.Enrollment.BLL.DTOs;

public class EnrollmentStatusHistoryDto
{
    public int HistoryId { get; set; }
    public int EnrollmentId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}