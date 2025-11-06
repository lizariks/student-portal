using StudentPortal.AggregatorService.Services;
using StudentPortal.AggregatorService.Services; // Доступ до AggregatedCourseDto
using Microsoft.AspNetCore.Mvc;


namespace StudentPortal.AggregatorService.Controllers;
    [ApiController]
    [Route("api/aggregated/courses")]
    public class CourseAggregatorController : ControllerBase
    {
        private readonly CourseAggregatorService _aggregatorService;
        private readonly ILogger<CourseAggregatorController> _logger;

        public CourseAggregatorController(
            CourseAggregatorService aggregatorService,
            ILogger<CourseAggregatorController> logger)
        {
            _aggregatorService = aggregatorService;
            _logger = logger;
        }

        /// <summary>
        /// Отримує агреговану інформацію для сторінки курсу (деталі та відгуки).
        /// </summary>
        /// <param name="courseId">ID курсу.</param>
        [HttpGet("{courseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseDetailsById(int courseId)
        {
            _logger.LogInformation("Receiving request for Aggregated Course ID: {CourseId}", courseId);
            
            try
            {
                // Тут ми не використовуємо CancellationToken, оскільки вона не була вказана у сигнатурі service
                var result = await _aggregatorService.GetAggregatedCourseByIdAsync(courseId);
                
                if (result is null)
                {
                    _logger.LogWarning("Aggregated data for Course ID {CourseId} not found.", courseId);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Логування критичних помилок (наприклад, збій Course Catalog Client)
                _logger.LogError(ex, "Critical error during aggregation for Course ID {CourseId}.", courseId);
                // Повертаємо 500, оскільки це внутрішній збій агрегатора
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal aggregation error occurred." });
            }
        }
    }