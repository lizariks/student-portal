namespace StudentPortal.Shared.Events.Modules;

public record ModuleDeletedEvent
{
    public int ModuleId { get; init; }
    public int CourseId { get; init; } 
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
}