namespace StudentPortal.Enrollment.Domain.Entities;

public class LessonProgress
{
    public int ProgressId { get; set; }
    public int StudentId { get; set; }
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public DateTime CompletedAt { get; set; }
}
