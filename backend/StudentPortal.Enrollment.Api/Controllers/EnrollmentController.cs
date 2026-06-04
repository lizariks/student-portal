namespace StudentPortal.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
using StudentPortal.Enrollment.Domain.Exceptions;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _service;
        public EnrollmentController(IEnrollmentService service) => _service = service;

        [HttpGet]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<Enrollment>> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("by-student/{studentId}")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetByStudent(int studentId)
            => Ok(await _service.GetByStudentAsync(studentId));

        [HttpGet("by-course/{courseId}")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetByCourse(int courseId)
            => Ok(await _service.GetByCourseAsync(courseId));

        [HttpPost("enroll")]
        [RequirePermission("enrollment:write")]
        public async Task<ActionResult<Enrollment>> EnrollStudent(int studentId, int courseId)
        {
            var enrollment = await _service.EnrollStudentAsync(studentId, courseId);
            return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId }, enrollment);
        }

        [HttpPut("{id}/status")]
        [RequirePermission("enrollment:write")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            await _service.UpdateStatusAsync(id, status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequirePermission("enrollment:delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
