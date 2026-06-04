namespace StudentPortal.IdentityService.BLL.Interfaces;
using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.Domain.Entities;
public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokensAsync(TokenRequestDto tokenRequestDto, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}