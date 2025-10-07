namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;


    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateUserAsync(int userId, User updatedUser, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<bool> HasRoleAsync(int userId, string roleName);
        Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId);

        Task<IEnumerable<StudentCourse>> GetUserEnrollmentsAsync(int userId);
    }
