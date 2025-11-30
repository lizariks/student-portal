namespace StudentPortal.Shared.Events.Lessons;

    public record LessonCreatedEvent
    {
        public int LessonId { get; init; }
        public int ModuleId { get; init; }
        public int CourseId { get; init; } 
        public string Title { get; init; } = null!;
        public TimeSpan? EstimatedDuration { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }