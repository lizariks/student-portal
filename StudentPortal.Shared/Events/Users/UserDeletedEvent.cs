namespace StudentPortal.Shared.Events.Users;

public record UserDeletedEvent
{
    public int UserId { get; init; }
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
}