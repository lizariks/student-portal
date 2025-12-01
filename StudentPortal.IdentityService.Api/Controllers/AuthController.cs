namespace StudentPortal.IdentityService.Api.Controllers;

using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.BLL.Interfaces;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("User registration requested for {Email}", registerDto.Email);
            var result = await _userService.RegisterUserAsync(registerDto, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _logger.LogInformation("User {Email} registered successfully.", registerDto.Email);
            return CreatedAtAction(nameof(Register), new { registerDto.Email }, null);
        }

        /// <summary>
        /// Authenticate a user and issue access/refresh tokens.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("User {Email} attempting to log in.", loginDto.Email);
            var tokens = await _userService.LoginUserAsync(loginDto, cancellationToken);
            return Ok(tokens);
        }

        /// <summary>
        /// Get info about the currently authenticated user.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found while requesting /me endpoint.", userId);
                return NotFound(new { message = "User not found." });
            }

            _logger.LogInformation("User {UserId} accessed /me endpoint.", userId);
            return Ok(user);
        }

        /// <summary>
        /// Refresh the access token using a valid refresh token.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] TokenRequestDto tokenRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Token refresh requested.");
            var newTokens = await _tokenService.RefreshTokensAsync(tokenRequest, cancellationToken);
            return Ok(newTokens);
        }

        /// <summary>
        /// Revoke (invalidate) a specific refresh token.
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revoke([FromBody] TokenRequestDto tokenRequest, CancellationToken cancellationToken)
        {
            var success = await _tokenService.RevokeTokenAsync(tokenRequest.RefreshToken, cancellationToken);

            if (!success)
                return BadRequest(new { message = "Invalid or already revoked token." });

            _logger.LogInformation("Successfully revoked refresh token for user {User}", User.Identity?.Name);
            return NoContent();
        }
        /// <summary>
        /// Confirm user's email address using a verification token.
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Email verification requested for user ID {UserId}", verifyDto.UserId);
        
            var result = await _userService.ConfirmEmailAsync(verifyDto, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors); 
            }

            _logger.LogInformation("User {UserId} successfully verified email.", verifyDto.UserId);
            return NoContent();
        }

        /// <summary>
        /// Log out current user (revoke all active refresh tokens).
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);
            _logger.LogInformation("User {UserId} logged out successfully (all tokens revoked).", userId);

            return NoContent();
        }
        
    }