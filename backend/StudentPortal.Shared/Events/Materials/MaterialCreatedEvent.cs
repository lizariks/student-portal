namespace StudentPortal.Shared.Events.Materials;
    public record MaterialCreatedEvent
    {
        public int MaterialId { get; init; }
        public int LessonId { get; init; }
        public int CourseId { get; init; } 
        public string Title { get; init; } = null!;
        public string Type { get; init; } = null!;
        public int Order { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }