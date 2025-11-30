namespace StudentPortal.Shared.Events.Lessons;


    public record LessonDeletedEvent
    {
        public int LessonId { get; init; }
        public int ModuleId { get; init; }
        public int CourseId { get; init; } 
        public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
    }