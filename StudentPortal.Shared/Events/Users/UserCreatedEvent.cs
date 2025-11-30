namespace StudentPortal.Shared.Events.Users;

public record UserCreatedEvent
{
    public int UserId { get; init; }
    public string Email { get; init; } = null!;
    public string Nickname { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}