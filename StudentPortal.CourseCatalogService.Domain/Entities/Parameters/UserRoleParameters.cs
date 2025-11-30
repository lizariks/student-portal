namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class UserRoleParameters : QueryStringParameters
{
    public int? UserId { get; set; }
    public int? RoleId { get; set; }
    public DateTime? AssignedAt { get; set; } = DateTime.UtcNow;
}