namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class StudentCourseParameters : QueryStringParameters
{
    public int? UserId { get; set; }
    public int? CourseId { get; set; }
    public DateTime? EnrolledAfter { get; set; }
    public DateTime? EnrolledBefore { get; set; }
    public string? OrderBy { get; set; } = "EnrolledAt"; 
}