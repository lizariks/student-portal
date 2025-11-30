namespace StudentPortal.Shared.Events.Roles;

public record RoleCreatedEvent
{
    public int RoleId { get; init; }
    public string Name { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}