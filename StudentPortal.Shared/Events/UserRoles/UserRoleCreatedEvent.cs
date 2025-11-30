namespace StudentPortal.Shared.Events.UserRoles;

public record UserRoleCreatedEvent
{
    public int UserId { get; init; }
    public int RoleId { get; init; } 
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Guid EventId { get; init; } = Guid.NewGuid();
}