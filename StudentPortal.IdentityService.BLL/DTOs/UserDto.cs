namespace StudentPortal.IdentityService.BLL.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}