namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class RoleParameters : QueryStringParameters
{
    public string? SearchKeyword { get; set; }
    public string? OrderBy { get; set; }
}
