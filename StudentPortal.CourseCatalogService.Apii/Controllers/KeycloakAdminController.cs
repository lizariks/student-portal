namespace StudentPortal.CourseCatalogService.APii.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.Apii.Services;

public record AdminRegisterRequest(string UserName, string Email, string Password, string Role);
public record AdminRoleRequest(string UserId, string RoleName);

[ApiController]
[Route("api/admin")]
[AllowAnonymous]
public class KeycloakAdminController : ControllerBase
{
    private readonly KeycloakAdminService _keycloakAdmin;

    public KeycloakAdminController(KeycloakAdminService keycloakAdmin)
    {
        _keycloakAdmin = keycloakAdmin;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _keycloakAdmin.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { error = "Failed to reach Keycloak admin API.", detail = ex.Message });
        }
    }

    [HttpPost("users/register")]
    public async Task<IActionResult> RegisterUser([FromBody] AdminRegisterRequest dto)
    {
        try
        {
            await _keycloakAdmin.CreateUserAsync(dto.UserName, dto.Email, dto.Password, dto.Role);
            return Created(string.Empty, new { email = dto.Email, role = dto.Role });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new[] { ex.Message } });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { errors = new[] { ex.Message } });
        }
    }

    [HttpPost("users/roles/add")]
    public async Task<IActionResult> AddRole([FromBody] AdminRoleRequest dto)
    {
        try
        {
            await _keycloakAdmin.AddRoleAsync(dto.UserId, dto.RoleName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/roles/remove")]
    public async Task<IActionResult> RemoveRole([FromBody] AdminRoleRequest dto)
    {
        try
        {
            await _keycloakAdmin.RemoveRoleAsync(dto.UserId, dto.RoleName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}