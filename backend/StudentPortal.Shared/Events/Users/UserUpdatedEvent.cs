namespace StudentPortal.Shared.Events.Users;

public record UserUpdatedEvent
{
    public int UserId { get; init; }
    public string NewNickname { get; init; } = null!; 
    public string NewFirstName { get; init; } = null!;
    public string NewLastName { get; init; } = null!;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}