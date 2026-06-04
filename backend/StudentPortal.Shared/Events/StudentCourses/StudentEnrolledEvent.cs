namespace StudentPortal.Shared.Events.StudentCourses;

public record StudentEnrolledEvent
{
    public int UserId { get; init; }
    public int CourseId { get; init; } 
    public DateTime EnrolledAt { get; init; } = DateTime.UtcNow;
}