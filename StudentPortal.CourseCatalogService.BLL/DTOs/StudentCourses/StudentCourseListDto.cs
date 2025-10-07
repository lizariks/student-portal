namespace StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;

public class StudentCourseListDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
}