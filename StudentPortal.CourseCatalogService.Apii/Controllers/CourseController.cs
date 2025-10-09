using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

namespace StudentPortal.CourseCatalogService.APii.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Get all courses with pagination and sorting
        /// </summary>
        /// <param name="parameters">Pagination and filtering parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paged list of courses</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedList<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedList<CourseListDto>>> GetPagedCourses(
            [FromQuery] CourseParameters parameters,
            CancellationToken cancellationToken)
        {
            var courses = await _courseService.GetPagedCoursesAsync(parameters, null, cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Get all courses
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all courses</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetAllCourses(CancellationToken cancellationToken)
        {
            var courses = await _courseService.GetAllCoursesAsync(cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Get course by ID with full details
        /// </summary>
        /// <param name="id">Course identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Course details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CourseDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDetailsDto>> GetCourseById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id, cancellationToken);
                return Ok(course);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get courses by instructor ID
        /// </summary>
        /// <param name="instructorId">Instructor identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of courses taught by the instructor</returns>
        [HttpGet("instructor/{instructorId:int}")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCoursesByInstructor(
            int instructorId,
            CancellationToken cancellationToken)
        {
            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId, cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Search courses by keyword
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching courses</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> SearchCourses(
            [FromQuery] string keyword,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Keyword cannot be empty." });

            var courses = await _courseService.SearchCoursesAsync(keyword, cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Get all published courses
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of published courses</returns>
        [HttpGet("published")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetPublishedCourses(CancellationToken cancellationToken)
        {
            var courses = await _courseService.GetPublishedCoursesAsync(cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Get all unpublished courses
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of unpublished courses</returns>
        [HttpGet("unpublished")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetUnpublishedCourses(CancellationToken cancellationToken)
        {
            var courses = await _courseService.GetUnpublishedCoursesAsync(cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Get courses with more than specified number of students
        /// </summary>
        /// <param name="count">Minimum number of students</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of courses with more than N students</returns>
        [HttpGet("students/more-than/{count:int}")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCoursesWithMoreThanNStudents(
            int count,
            CancellationToken cancellationToken)
        {
            if (count < 0)
                return BadRequest(new { message = "Count must be a non-negative number." });

            var courses = await _courseService.GetCoursesWithMoreThanNStudentsAsync(count, cancellationToken);
            return Ok(courses);
        }

        /// <summary>
        /// Create a new course
        /// </summary>
        /// <param name="dto">Course creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created course</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CourseDto>> CreateCourse(
            [FromBody] CourseCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var course = await _courseService.CreateCourseAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing course
        /// </summary>
        /// <param name="id">Course identifier</param>
        /// <param name="dto">Course update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated course</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CourseDto>> UpdateCourse(
            int id,
            [FromBody] CourseUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var course = await _courseService.UpdateCourseAsync(id, dto, cancellationToken);
                return Ok(course);
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
        /// Publish a course
        /// </summary>
        /// <param name="id">Course identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpPatch("{id:int}/publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PublishCourse(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _courseService.PublishCourseAsync(id, cancellationToken);
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
        /// Unpublish a course
        /// </summary>
        /// <param name="id">Course identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpPatch("{id:int}/unpublish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UnpublishCourse(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _courseService.UnpublishCourseAsync(id, cancellationToken);
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
        /// Delete a course
        /// </summary>
        /// <param name="id">Course identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _courseService.DeleteCourseAsync(id, cancellationToken);
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
}