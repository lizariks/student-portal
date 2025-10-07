namespace StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
public class StudentCourseDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }

    public UserListDto User { get; set; } = null!;
    public CourseListDto Course { get; set; } = null!;
}