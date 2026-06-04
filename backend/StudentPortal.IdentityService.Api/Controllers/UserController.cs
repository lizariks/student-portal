namespace StudentPortal.IdentityService.Api.Controllers;

using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


    [ApiController]
    [Route("api/admin/users")]
    [AllowAnonymous]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all registered users.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin requested user list");
            var users = await _userService.GetUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get a specific user by ID.
        /// </summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin requested user {UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
                return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        /// <summary>
        /// Get all refresh tokens for a specific user.
        /// </summary>
        [HttpGet("{userId:guid}/tokens")]
        [ProducesResponseType(typeof(IEnumerable<RefreshTokenDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RefreshTokenDto>>> GetUserTokens(Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin requested refresh tokens for user {UserId}", userId);
            var tokens = await _userService.GetUserRefreshTokensAsync(userId, cancellationToken);
            return Ok(tokens);
        }

        /// <summary>
        /// Add a user to a specific role.
        /// </summary>
        [HttpPost("roles/add")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddUserToRole([FromBody] UserRoleDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin adding user {UserId} to role {Role}", dto.UserId, dto.RoleName);
            var result = await _userService.AddUserToRoleAsync(dto.UserId, dto.RoleName, cancellationToken);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return NoContent();
        }

        /// <summary>
        /// Remove a user from a specific role.
        /// </summary>
        [HttpPost("roles/remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveUserFromRole([FromBody] UserRoleDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin removing user {UserId} from role {Role}", dto.UserId, dto.RoleName);
            var result = await _userService.RemoveUserFromRoleAsync(dto.UserId, dto.RoleName, cancellationToken);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return NoContent();
        }

        /// <summary>
        /// Register a new user with an assigned role (admin only).
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterWithRole([FromBody] AdminRegisterDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Admin registering new user {Email} with role {Role}", dto.Email, dto.Role);
            var (success, userId, errors) = await _userService.RegisterWithRoleAsync(dto, cancellationToken);

            if (!success)
                return BadRequest(new { errors });

            return Created(string.Empty, new { id = userId, email = dto.Email, role = dto.Role });
        }
    }
