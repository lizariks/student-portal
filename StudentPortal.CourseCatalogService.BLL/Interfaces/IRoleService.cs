namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public interface IRoleService
{
    Task<RoleDto> CreateRoleAsync(RoleCreateDto roleCreateDto, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRoleAsync(int roleId, RoleUpdateDto roleUpdateDto, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RoleDto?> GetRoleByIdAsync(int roleId);
    Task<IEnumerable<RoleListDto>> GetAllRolesAsync();
    Task<PagedList<RoleListDto>> GetPagedRolesAsync(
        RoleParameters parameters,
        ISortHelper<Role>? sortHelper = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDto>> GetUsersInRoleAsync(int roleId);
    Task<bool> RoleExistsAsync(string roleName);
}
