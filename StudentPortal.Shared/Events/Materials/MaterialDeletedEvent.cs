namespace StudentPortal.Shared.Events.Materials;

public record MaterialDeletedEvent
{
    public int MaterialId { get; init; }
    public int LessonId { get; init; }
    public int CourseId { get; init; } 
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
}