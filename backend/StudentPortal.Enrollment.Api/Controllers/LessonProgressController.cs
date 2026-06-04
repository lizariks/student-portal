namespace StudentPortal.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Enrollment.BLL.Interfaces;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LessonProgressController : ControllerBase
{
    private readonly ILessonProgressService _service;
    public LessonProgressController(ILessonProgressService service) => _service = service;

    [HttpGet("student/{studentId}/course/{courseId}")]
    public async Task<IActionResult> GetByStudentAndCourse(int studentId, int courseId)
        => Ok(await _service.GetByStudentAndCourseAsync(studentId, courseId));

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetByCourse(int courseId)
        => Ok(await _service.GetByCourseAsync(courseId));

    [HttpPost]
    public async Task<IActionResult> MarkComplete([FromBody] MarkCompleteRequest req)
    {
        await _service.MarkCompleteAsync(req.StudentId, req.LessonId, req.CourseId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> MarkIncomplete([FromQuery] int studentId, [FromQuery] int lessonId)
    {
        await _service.MarkIncompleteAsync(studentId, lessonId);
        return NoContent();
    }
}

public record MarkCompleteRequest(int StudentId, int LessonId, int CourseId);
