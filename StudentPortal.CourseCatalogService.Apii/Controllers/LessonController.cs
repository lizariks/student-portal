namespace StudentPortal.CourseCatalogService.APii.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        /// <summary>
        /// Get all lessons with pagination and sorting
        /// </summary>
        /// <param name="parameters">Pagination and filtering parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paged list of lessons</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedList<LessonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedList<LessonDto>>> GetPagedLessons(
            [FromQuery] LessonParameters parameters,
            CancellationToken cancellationToken)
        {
            var lessons = await _lessonService.GetPagedLessonsAsync(parameters, null, cancellationToken);
            return Ok(lessons);
        }

        /// <summary>
        /// Get all lessons
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all lessons</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LessonDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetAllLessons(CancellationToken cancellationToken)
        {
            var lessons = await _lessonService.GetAllLessonsAsync(cancellationToken);
            return Ok(lessons);
        }

        /// <summary>
        /// Get lesson by ID
        /// </summary>
        /// <param name="id">Lesson identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Lesson details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LessonDto>> GetLessonById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id, cancellationToken);
                return Ok(lesson);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get lessons by module ID
        /// </summary>
        /// <param name="moduleId">Module identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of lessons in the module</returns>
        [HttpGet("module/{moduleId:int}")]
        [ProducesResponseType(typeof(IEnumerable<LessonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByModule(
            int moduleId,
            CancellationToken cancellationToken)
        {
            try
            {
                var lessons = await _lessonService.GetLessonsByModuleAsync(moduleId, cancellationToken);
                return Ok(lessons);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new lesson
        /// </summary>
        /// <param name="dto">Lesson creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created lesson</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LessonDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LessonDto>> CreateLesson(
            [FromBody] LessonCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var lesson = await _lessonService.CreateLessonAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetLessonById), new { id = lesson.Id }, lesson);
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

        /// <summary>
        /// Update an existing lesson
        /// </summary>
        /// <param name="id">Lesson identifier</param>
        /// <param name="dto">Lesson update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated lesson</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LessonDto>> UpdateLesson(
            int id,
            [FromBody] LessonUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var lesson = await _lessonService.UpdateLessonAsync(id, dto, cancellationToken);
                return Ok(lesson);
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

        /// <summary>
        /// Reorder a lesson within its module
        /// </summary>
        /// <param name="lessonId">Lesson identifier</param>
        /// <param name="newOrder">New order position</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpPatch("{lessonId:int}/reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReorderLesson(
            int lessonId,
            [FromQuery] int newOrder,
            CancellationToken cancellationToken)
        {
            if (newOrder < 1)
                return BadRequest(new { message = "New order must be greater than 0." });

            try
            {
                await _lessonService.ReorderLessonAsync(lessonId, newOrder, cancellationToken);
                return NoContent();
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

        /// <summary>
        /// Delete a lesson
        /// </summary>
        /// <param name="id">Lesson identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteLesson(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _lessonService.DeleteLessonAsync(id, cancellationToken);
                return NoContent();
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
    }