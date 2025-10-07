namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class RolesWithFiltersSpecification : Specification<Role>
{
    public RolesWithFiltersSpecification(RoleParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
        {
            Query.Where(r => r.Name.Contains(parameters.SearchKeyword));
        }

        Query.Include(r => r.UserRoles)
            .ThenInclude(ur => ur.User)
            .AsSplitQuery();
    }
}