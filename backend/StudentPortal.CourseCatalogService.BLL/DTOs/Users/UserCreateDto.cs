namespace StudentPortal.CourseCatalogService.BLL.DTOs.Users;

public class UserCreateDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}