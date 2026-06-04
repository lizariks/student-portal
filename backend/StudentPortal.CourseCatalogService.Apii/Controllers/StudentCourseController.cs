using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace StudentPortal.CourseCatalogService.APii.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentCoursesController : ControllerBase
{
    private readonly IStudentCourseService _studentCourseService;

    public StudentCoursesController(IStudentCourseService studentCourseService)
    {
        _studentCourseService = studentCourseService;
    }

    [HttpPost("enroll")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StudentCourseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> EnrollAsync([FromBody] StudentCourseCreateDto dto)
    {
        try
        {
            var result = await _studentCourseService.EnrollStudentAsync(dto);
            return Created(string.Empty, result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("unenroll")]
    [AllowAnonymous]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnenrollAsync([FromQuery] int userId, [FromQuery] int courseId)
    {
        try
        {
            await _studentCourseService.UnenrollStudentAsync(userId, courseId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("paged")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedList<StudentCourseDto>), 200)]
    public async Task<IActionResult> GetPagedAsync([FromQuery] StudentCourseParameters parameters)
    {
        var result = await _studentCourseService.GetPagedStudentCoursesAsync(parameters);
        return Ok(result);
    }

    [HttpGet("user/{userId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<StudentCourseDto>), 200)]
    public async Task<IActionResult> GetByUserAsync(int userId)
    {
        var result = await _studentCourseService.GetEnrollmentsByUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("course/{courseId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<StudentCourseDto>), 200)]
    public async Task<IActionResult> GetByCourseAsync(int courseId)
    {
        var result = await _studentCourseService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(result);
    }

    [HttpGet("course/{courseId:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> GetEnrollmentCountAsync(int courseId)
    {
        var count = await _studentCourseService.GetEnrollmentCountForCourseAsync(courseId);
        return Ok(count);
    }

    [HttpGet("check")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> IsEnrolledAsync([FromQuery] int userId, [FromQuery] int courseId)
    {
        var enrolled = await _studentCourseService.IsUserEnrolledAsync(userId, courseId);
        return Ok(enrolled);
    }
}
