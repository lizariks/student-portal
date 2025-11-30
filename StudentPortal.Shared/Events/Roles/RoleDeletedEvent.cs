namespace StudentPortal.Shared.Events.Roles;

    public record RoleDeletedEvent
    {
        public int RoleId { get; init; }
        public string Name { get; init; } = null!;
        public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
    }