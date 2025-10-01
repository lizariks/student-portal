namespace StudentPortal.CourseCatalogService.Domain.Entities;

public class Module
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; } 

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public Course? Course { get; set; }
}