namespace StudentPortal.Shared.Events.UserRoles;

public record UserRoleDeletedEvent
{
    public int UserId { get; init; } 
    public int RoleId { get; init; } 
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
    
    public Guid EventId { get; init; } = Guid.NewGuid();
}