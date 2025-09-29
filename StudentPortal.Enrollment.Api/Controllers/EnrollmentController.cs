namespace StudentPortal.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.Domain.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _service;
        public EnrollmentController(IEnrollmentService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("by-student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetByStudent(int studentId)
            => Ok(await _service.GetByStudentAsync(studentId));

        [HttpGet("by-course/{courseId}")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetByCourse(int courseId)
            => Ok(await _service.GetByCourseAsync(courseId));

        [HttpPost("enroll")]
        public async Task<ActionResult<Enrollment>> EnrollStudent(int studentId, int courseId)
        {
            var enrollment = await _service.EnrollStudentAsync(studentId, courseId);
            return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId }, enrollment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            await _service.UpdateStatusAsync(id, status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
