namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class UserParameters : QueryStringParameters
{
    public string? SearchKeyword { get; set; }
    public string? OrderBy { get; set; }
}