namespace StudentPortal.IdentityService.BLL.Services;

using AutoMapper;
using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.BLL.Interfaces;
using StudentPortal.IdentityService.DAL.Interfaces;
using StudentPortal.IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;


    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<ApplicationUser>(registerDto);

            _logger.LogInformation("Attempting to register new user {Email}", registerDto.Email);
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created. Adding to 'User' role.", registerDto.Email);
                await _userManager.AddToRoleAsync(user, "User");
            }

            return result;
        }

        public async Task<(bool Success, Guid? UserId, IEnumerable<string> Errors)> RegisterWithRoleAsync(AdminRegisterDto dto, CancellationToken cancellationToken = default)
        {
            var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, "User");

            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role != "User")
            {
                if (await _roleManager.RoleExistsAsync(dto.Role))
                    await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return (true, user.Id, Enumerable.Empty<string>());
        }

        public async Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found.", loginDto.Email);
                throw new AuthenticationException("Invalid email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}.", loginDto.Email);
                throw new AuthenticationException("Invalid email or password.");
            }

            _logger.LogInformation("User {Email} logged in successfully. Generating tokens.", loginDto.Email);
            return await _tokenService.GenerateTokensAsync(user, cancellationToken);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);
            var userDtos = _mapper.Map<List<UserDto>>(users);

            foreach (var userDto in userDtos)
            {
                var user = users.First(u => u.Id == userDto.Id);
                userDto.Roles = await _userManager.GetRolesAsync(user);
            }

            return userDtos;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = await _userManager.GetRolesAsync(user);
            return userDto;
        }

        public async Task<IEnumerable<RefreshTokenDto>> GetUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _refreshTokenRepository.GetUserTokensAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<RefreshTokenDto>>(tokens);
        }

        public async Task<IdentityResult> AddUserToRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            if (!await _roleManager.RoleExistsAsync(roleName))
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }
        public async Task<IdentityResult> ConfirmEmailAsync(VerifyEmailDto verifyDto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(verifyDto.UserId.ToString());

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Verification failed: User not found." });
            }
        
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for user {UserId}.", verifyDto.UserId);
                return IdentityResult.Success;
            }

            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(verifyDto.Token);
                var decodedToken = Encoding.UTF8.GetString(tokenBytes);

                _logger.LogInformation("Attempting to confirm email for user {UserId}", verifyDto.UserId);
            
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            
                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmed successfully for user {UserId}.", verifyDto.UserId);
                }
                else
                {
                    _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", verifyDto.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (FormatException)
            {
                _logger.LogError("Verification failed: Invalid token format provided by user {UserId}.", verifyDto.UserId);
                return IdentityResult.Failed(new IdentityError { Description = "Verification failed: Invalid token format." });
            }
    }
    }
