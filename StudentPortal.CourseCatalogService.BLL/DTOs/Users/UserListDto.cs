namespace StudentPortal.CourseCatalogService.BLL.DTOs.Users;

public class UserListDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public int CoursesTaughtCount { get; set; }
    public int CoursesEnrolledCount { get; set; }
}