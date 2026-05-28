namespace StudentPortal.IdentityService.BLL.Interfaces;
using StudentPortal.IdentityService.BLL.DTOs;
using Microsoft.AspNetCore.Identity;

public interface IUserService
{
    Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<(bool Success, Guid? UserId, IEnumerable<string> Errors)> RegisterWithRoleAsync(AdminRegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshTokenDto>> GetUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IdentityResult> AddUserToRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<IdentityResult> RemoveUserFromRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<IdentityResult> ConfirmEmailAsync(VerifyEmailDto verifyDto, CancellationToken cancellationToken = default);
}