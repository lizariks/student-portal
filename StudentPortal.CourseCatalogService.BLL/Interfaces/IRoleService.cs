namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;


    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> UpdateRoleAsync(int roleId, Role updatedRole, CancellationToken cancellationToken = default);
        Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<IEnumerable<Role>> GetAllRolesAsync();

        Task<IEnumerable<UserRole>> GetUsersInRoleAsync(int roleId);
        Task<bool> RoleExistsAsync(string roleName);
    }
