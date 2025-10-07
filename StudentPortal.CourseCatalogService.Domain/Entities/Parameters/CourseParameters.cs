namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class CourseParameters : QueryStringParameters
{
    public string? SearchKeyword { get; set; }
    public int? InstructorId { get; set; }
    public bool? IsPublished { get; set; }
    public string? OrderBy { get; set; }
   
}