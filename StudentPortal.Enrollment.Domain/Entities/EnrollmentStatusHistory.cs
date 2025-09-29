namespace StudentPortal.Enrollment.Domain;

public class EnrollmentStatusHistory
{
    public int HistoryId { get; set; }
    public int EnrollmentId { get; set; }

    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }

    public Enrollment? Enrollment { get; set; }
}
