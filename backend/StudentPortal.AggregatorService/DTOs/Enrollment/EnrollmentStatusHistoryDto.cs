namespace StudentPortal.AggregatorService.DTOs.Enrollment;

public class EnrollmentStatusHistoryDto
{
    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}