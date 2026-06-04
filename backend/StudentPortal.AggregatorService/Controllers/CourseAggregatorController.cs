using StudentPortal.AggregatorService.Services;
using Microsoft.AspNetCore.Mvc;


namespace StudentPortal.AggregatorService.Controllers;

[ApiController]
[Route("api/aggregated/courses")]
public class CourseAggregatorController : ControllerBase
{
    private readonly CourseAggregatorService _aggregatorService;

    public CourseAggregatorController(CourseAggregatorService aggregatorService)
    {
        _aggregatorService = aggregatorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _aggregatorService.GetAllCoursesAggregatedAsync();
        // Примітка: AggregatorService.GetAllCoursesAggregatedAsync повертає List<T>,
        // тому перевірка на null тут менш імовірна, але залишаємо логіку прикладу.
        return result is null ? NotFound() : Ok(result); 
    }

    [HttpGet("{courseId:int}")]
    public async Task<IActionResult> GetById(int courseId)
    {
        var result = await _aggregatorService.GetAggregatedCourseByIdAsync(courseId);
        return result is null ? NotFound() : Ok(result);
    }
}