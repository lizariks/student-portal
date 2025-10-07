namespace StudentPortal.CourseCatalogService.BLL.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}