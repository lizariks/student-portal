using StudentPortal.AggregatorService.Services;
using Microsoft.AspNetCore.Mvc;


namespace StudentPortal.AggregatorService.Controllers;

[ApiController]
[Route("api/aggregated/enrollment")]
public class EnrollmentAggregatorController : ControllerBase
{
    private readonly EnrollmentAggregatorService _aggregatorService;

    public EnrollmentAggregatorController(EnrollmentAggregatorService aggregatorService)
    {
        _aggregatorService = aggregatorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // Метод сервісу вимагає CancellationToken
        var result = await _aggregatorService.GetAllEnrollmentsAggregatedAsync(ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{enrollmentId:int}")]
    public async Task<IActionResult> GetById(int enrollmentId, CancellationToken ct)
    {
        // Метод сервісу вимагає CancellationToken
        var result = await _aggregatorService.GetAggregatedEnrollmentByIdAsync(enrollmentId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}