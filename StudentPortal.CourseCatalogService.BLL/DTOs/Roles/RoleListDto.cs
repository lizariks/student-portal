namespace StudentPortal.CourseCatalogService.BLL.DTOs.Roles;

public class RoleListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int UsersCount { get; set; }
}