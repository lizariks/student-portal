
using StudentPortal.AggregatorService.DTOs.CourseCatalog; 

namespace StudentPortal.AggregatorService.Clients;

public class CourseCatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CourseCatalogClient> _logger;

    public CourseCatalogClient(HttpClient httpClient, ILogger<CourseCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Отримує список усіх курсів.
    /// </summary>
    public async Task<List<CourseDto>?> GetAllCoursesAsync()
    {
        _logger.LogInformation("Requesting all course catalog data.");
        return await _httpClient.GetFromJsonAsync<List<CourseDto>>("/api/catalog");
    }

    /// <summary>
    /// Отримує деталі курсу та його структуру за CourseId.
    /// </summary>
    public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
    {
        _logger.LogInformation("Requesting course catalog data for ID: {CourseId}", courseId);
        return await _httpClient.GetFromJsonAsync<CourseDto>($"/api/catalog/{courseId}");
    }
}