namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;


    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken cancellationToken = default);

        Task<UserDto> UpdateUserAsync(int userId, UserUpdateDto userUpdateDto,
            CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<bool> HasRoleAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);

        Task<IEnumerable<StudentCourseDto>> GetUserEnrollmentsAsync(int userId);
    }
