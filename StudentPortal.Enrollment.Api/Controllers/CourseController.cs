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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;
        public CourseController(ICourseService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll(CancellationToken ct)
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<Course>> Create([FromBody] Course course)
        {
            var created = await _service.CreateAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = created.CourseId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Course course)
        {
            course.CourseId = id;
            await _service.UpdateAsync(course);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("with-enrollments")]
        public async Task<ActionResult<IEnumerable<Course>>> GetWithEnrollments()
            => Ok(await _service.GetCoursesWithEnrollmentsAsync());
    }