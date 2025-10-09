using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;

namespace StudentPortal.CourseCatalogService.APii.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleListDto>), 200)]
    public async Task<IActionResult> GetAllAsync()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateAsync([FromBody] RoleCreateDto dto)
    {
        try
        {
            var created = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
        }
        catch (BusinessException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] RoleUpdateDto dto)
    {
        try
        {
            var updated = await _roleService.UpdateRoleAsync(id, dto);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/users")]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUsersInRoleAsync(int id)
    {
        try
        {
            var users = await _roleService.GetUsersInRoleAsync(id);
            return Ok(users);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedList<RoleListDto>), 200)]
    public async Task<IActionResult> GetPagedAsync([FromQuery] RoleParameters parameters)
    {
        var paged = await _roleService.GetPagedRolesAsync(parameters);
        return Ok(paged);
    }
}
