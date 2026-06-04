
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.DTOs.UserRoles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

public interface IUserRoleService
{
    Task AssignRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);

    Task UnassignRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    
    Task<UserRoleDto?> GetByCompositeKeyAsync(int userId, int roleId, CancellationToken cancellationToken = default);
}