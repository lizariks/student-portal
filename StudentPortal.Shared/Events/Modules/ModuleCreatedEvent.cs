namespace StudentPortal.Shared.Events.Modules;

    public record ModuleCreatedEvent
    {
        public int ModuleId { get; init; }
        public int CourseId { get; init; } 
        public string Title { get; init; } = null!;
        public int Order { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }