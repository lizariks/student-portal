namespace StudentPortal.IdentityService.BLL.Services;

using StudentPortal.IdentityService.BLL.Config;
using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.BLL.Interfaces;
using StudentPortal.IdentityService.DAL.Interfaces;
using StudentPortal.IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;


    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResponseDto> RefreshTokensAsync(TokenRequestDto tokenRequestDto, CancellationToken cancellationToken = default)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(tokenRequestDto.RefreshToken, cancellationToken);

            if (storedToken == null)
                throw new AuthenticationException("Refresh token does not exist.");

            if (DateTime.UtcNow > storedToken.Expires)
                throw new AuthenticationException("Refresh token has expired.");

            if (storedToken.IsRevoked)
                throw new AuthenticationException("Refresh token has been revoked.");

            if (storedToken.IsUsed)
                throw new AuthenticationException("Refresh token has already been used.");

            storedToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

            var user = storedToken.User;
            return await GenerateTokensAsync(user, cancellationToken);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

            if (storedToken == null || storedToken.IsRevoked)
                return false;

            storedToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
            _logger.LogInformation("Revoked token {TokenId} for user {UserId}", storedToken.Id, storedToken.UserId);
            return true;
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _refreshTokenRepository.GetUserTokensAsync(userId, cancellationToken);
            if (!tokens.Any())
            {
                _logger.LogInformation("No refresh tokens found for user {UserId}", userId);
                return;
            }

            foreach (var token in tokens)
            {
                if (!token.IsRevoked)
                {
                    token.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
                }
            }

            _logger.LogInformation("Revoked all active refresh tokens for user {UserId}", userId);
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(ClaimTypes.Name, user.UserName!)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenDurationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
        {
            string token;
            RefreshToken? existingToken;

            do
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                token = Convert.ToBase64String(randomNumber);

                existingToken = await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
            } while (existingToken != null);

            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationDays),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            return refreshToken;
        }
    }
