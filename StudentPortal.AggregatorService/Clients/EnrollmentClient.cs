namespace StudentPortal.AggregatorService.Clients;

using StudentPortal.AggregatorService.DTOs.Enrollment; 

public class EnrollmentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnrollmentClient> _logger;

    public EnrollmentClient(HttpClient httpClient, ILogger<EnrollmentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Отримує всі записи про зарахування.
    /// </summary>
    public async Task<List<EnrollmentDto>?> GetAllEnrollmentsAsync()
    {
        _logger.LogInformation("Requesting all enrollment source data.");
        return await _httpClient.GetFromJsonAsync<List<EnrollmentDto>>("/api/enrollment");
    }

    /// <summary>
    /// Отримує дані про зарахування за його ID.
    /// </summary>
    public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        _logger.LogInformation("Requesting enrollment source data for ID: {EnrollmentId}", enrollmentId);
        return await _httpClient.GetFromJsonAsync<EnrollmentDto>($"/api/enrollment/{enrollmentId}");
    }
}