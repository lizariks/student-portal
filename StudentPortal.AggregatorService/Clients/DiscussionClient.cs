
using StudentPortal.AggregatorService.DTOs.Discussion;

namespace StudentPortal.AggregatorService.Clients;

public class DiscussionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DiscussionClient> _logger;

    public DiscussionClient(HttpClient httpClient, ILogger<DiscussionClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Отримує всі обговорення/відгуки.
    /// </summary>
    public async Task<List<DiscussionThreadDto>?> GetAllDiscussionsAsync()
    {
        _logger.LogInformation("Requesting all discussion threads.");
        return await _httpClient.GetFromJsonAsync<List<DiscussionThreadDto>>("/api/discussions");
    }

    /// <summary>
    /// Отримує конкретну гілку обговорення або відгук за ID.
    /// </summary>
    public async Task<DiscussionThreadDto?> GetDiscussionByIdAsync(string discussionId)
    {
        _logger.LogInformation("Requesting discussion thread for ID: {DiscussionId}", discussionId);
        return await _httpClient.GetFromJsonAsync<DiscussionThreadDto>($"/api/discussions/{discussionId}");
    }
    public async Task<CourseReviewDto?> GetReviewSummaryByCourseIdAsync(int courseId)
    {
        _logger.LogInformation("Requesting review summary for CourseId: {CourseId}", courseId);
        // Ми припускаємо, що Discussion Service має endpoint для агрегації
        return await _httpClient.GetFromJsonAsync<CourseReviewDto>($"/api/discussions/summary/course/{courseId}");
    }
}