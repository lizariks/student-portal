namespace StudentPortal.CourseCatalogService.BLL.DTOs.UserRoles;

public class UserRoleDto
{
    public int UserId { get; init; }
    public int RoleId { get; init; }

    public string? UserName { get; init; }
    
    public string? RoleName { get; init; } 
    
    public DateTime AssignedAt { get; init; }
}