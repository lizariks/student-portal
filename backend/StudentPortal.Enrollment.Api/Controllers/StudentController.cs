namespace StudentPortal.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
using StudentPortal.Enrollment.Domain.Exceptions;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _service;
        public StudentController(IStudentService service) => _service = service;

        [HttpGet("{id}")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<Student>> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("by-email")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<Student>> GetByEmail([FromQuery] string email)
        {
            var student = await _service.GetByEmailAsync(email);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpPost]
        [RequirePermission("enrollment:write")]
        public async Task<ActionResult<Student>> Create([FromBody] Student student)
        {
            var created = await _service.CreateAsync(student);
            return CreatedAtAction(nameof(GetById), new { id = created.StudentId }, created);
        }

        [HttpPut("{id}")]
        [RequirePermission("enrollment:write")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            student.StudentId = id;
            await _service.UpdateAsync(student);
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
