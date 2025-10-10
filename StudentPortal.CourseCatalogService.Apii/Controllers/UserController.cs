using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
namespace StudentPortal.CourseCatalogService.APii.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateAsync([FromBody] UserCreateDto dto)
    {
        try
        {
            var created = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetByIdAsync), new { userId = created.Id }, created);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:int}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAsync(int userId, [FromBody] UserUpdateDto dto)
    {
        try
        {
            var updated = await _userService.UpdateUserAsync(userId, dto);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{userId:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync(int userId)
    {
        await _userService.DeleteUserAsync(userId);
        return NoContent();
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByIdAsync(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    public async Task<IActionResult> GetAllAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
    
    [HttpPost("{userId:int}/roles/{roleId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignRoleAsync(int userId, int roleId)
    {
        try
        {
            await _userService.AssignRoleAsync(userId, roleId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{userId:int}/roles/{roleId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveRoleAsync(int userId, int roleId)
    {
        try
        {
            await _userService.RemoveRoleAsync(userId, roleId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:int}/roles")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRolesAsync(int userId)
    {
        try
        {
            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:int}/roles/check")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> HasRoleAsync(int userId, [FromQuery] string roleName)
    {
        try
        {
            var hasRole = await _userService.HasRoleAsync(userId, roleName);
            return Ok(hasRole);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:int}/enrollments")]
    [ProducesResponseType(typeof(IEnumerable<StudentCourseDto>), 200)]
    public async Task<IActionResult> GetEnrollmentsAsync(int userId)
    {
        var enrollments = await _userService.GetUserEnrollmentsAsync(userId);
        return Ok(enrollments);
    }
}
