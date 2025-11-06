namespace StudentPortal.AggregatorService.Controllers;

using StudentPortal.AggregatorService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

    [ApiController]
    [Route("api/aggregated/enrollment")]
    public class EnrollmentAggregatorController : ControllerBase
    {
        private readonly EnrollmentAggregatorService _aggregatorService;
        private readonly ILogger<EnrollmentAggregatorController> _logger;

        public EnrollmentAggregatorController(
            EnrollmentAggregatorService aggregatorService,
            ILogger<EnrollmentAggregatorController> logger)
        {
            _aggregatorService = aggregatorService;
            _logger = logger;
        }

        /// <summary>
        /// Отримує агреговані деталі запису студента (Enrollment + Course Info + Reviews).
        /// </summary>
        /// <param name="enrollmentId">ID запису на курс.</param>
        /// <param name="ct">CancellationToken, отриманий від ASP.NET Core.</param>
        [HttpGet("{enrollmentId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Для помилок агрегації/мережі
        public async Task<IActionResult> GetEnrollmentDetailsById(int enrollmentId, CancellationToken ct)
        {
            _logger.LogInformation("Receiving request for Aggregated Enrollment ID: {EnrollmentId}", enrollmentId);
            
            try
            {
                var result = await _aggregatorService.GetAggregatedEnrollmentByIdAsync(enrollmentId, ct);
                
                if (result is null)
                {
                    _logger.LogWarning("Aggregated data for Enrollment ID {EnrollmentId} not found.", enrollmentId);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Логування помилок Aggregator Service (наприклад, критичний збій у Enrollment Client)
                _logger.LogError(ex, "Critical error during aggregation for Enrollment ID {EnrollmentId}.", enrollmentId);
                // Повертаємо 500, оскільки це внутрішній збій агрегатора
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal aggregation error occurred." });
            }
        }
    }