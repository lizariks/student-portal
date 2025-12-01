namespace StudentPortal.IdentityService.BLL.DTOs;

public class UserRoleDto
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = null!;
}