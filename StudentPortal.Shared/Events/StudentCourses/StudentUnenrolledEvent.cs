namespace StudentPortal.Shared.Events.StudentCourses;

public record StudentUnenrolledEvent
{
    public int UserId { get; init; }
    public int CourseId { get; init; } 
    public DateTime UnenrolledAt { get; init; } = DateTime.UtcNow;
}