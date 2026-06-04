namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


public class UserRoleWithFiltersSpecification : Specification<UserRole>
{
    public UserRoleWithFiltersSpecification(UserRoleParameters parameters)
    {
        if (parameters.UserId.HasValue)
            Query.Where(ur => ur.UserId == parameters.UserId.Value);

        if (parameters.RoleId.HasValue) 
            Query.Where(ur => ur.RoleId == parameters.RoleId.Value);

        Query.Include(ur => ur.User)
             .Include(ur => ur.Role)
             .AsSplitQuery();
        
    }
}