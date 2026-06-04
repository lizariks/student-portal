using StudentPortal.AggregatorService.Clients;
using StudentPortal.AggregatorService.DTOs.Aggregated; 
using StudentPortal.AggregatorService.DTOs.CourseCatalog; 
using StudentPortal.AggregatorService.DTOs.Discussion; 


namespace StudentPortal.AggregatorService.Services;
    public class CourseAggregatorService
    {
        private readonly CourseCatalogGrpcClient _catalogClient;
        private readonly DiscussionGrpcClient _discussionClient;
        private readonly ILogger<CourseAggregatorService> _logger;

        public CourseAggregatorService(
            CourseCatalogGrpcClient catalogClient,
            DiscussionGrpcClient discussionClient,
            ILogger<CourseAggregatorService> logger)
        {
            _catalogClient = catalogClient;
            _discussionClient = discussionClient;
            _logger = logger;
        }

        public async Task<List<AggregatedCourseDto>> GetAllCoursesAggregatedAsync()
        {
            List<CourseDto>? courses;
            try
            {
                courses = await _catalogClient.GetAllCoursesAsync();
                if (courses is null || !courses.Any())
                {
                    _logger.LogWarning("No courses found in the catalog.");
                    return new List<AggregatedCourseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch base course data. Aborting aggregation.");
                throw; 
            }

            var warnings = new List<string>();

            var reviewTasks = courses.Select(c => GetCourseReviewSummaryAsync(c.Id, warnings)).ToList();

            await Task.WhenAll(reviewTasks);

            var reviewSummaries = reviewTasks.Select(t => t.Result).ToList();

            var aggregatedCourses = courses.Select((course, index) =>
            {
                var reviewSummary = reviewSummaries[index];
                
                return new AggregatedCourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Code = course.Code,
                    Description = course.Description,
                    CreatedAt = course.CreatedAt,
                    InstructorId = course.InstructorId,
                    
                    AverageRating = reviewSummary?.AverageRating,
                    TotalReviews = reviewSummary?.TotalReviews ?? 0,
                };
            }).ToList();

            if (warnings.Any())
            {
                _logger.LogWarning("Aggregation completed with warnings: {Warnings}", string.Join("; ", warnings));
            }
            _logger.LogInformation("Successfully aggregated {Count} courses.", aggregatedCourses.Count);
            return aggregatedCourses;
        }

        public async Task<AggregatedCourseDto?> GetAggregatedCourseByIdAsync(int courseId)
        {
            CourseDto? course = null;
            try
            {
                course = await _catalogClient.GetCourseByIdAsync(courseId);
                if (course is null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found in catalog.", courseId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch base course data for ID {CourseId}. Aborting aggregation.", courseId);
                throw; 
            }

            var warnings = new List<string>();

            var reviewsTask = GetCourseReviewSummaryAsync(courseId, warnings);

            await Task.WhenAll(reviewsTask);

            var reviewSummary = await reviewsTask;
            var dto = new AggregatedCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Code = course.Code,
                Description = course.Description,
                CreatedAt = course.CreatedAt,
                InstructorId = course.InstructorId,
                AverageRating = reviewSummary?.AverageRating,
                TotalReviews = reviewSummary?.TotalReviews ?? 0,
                
            };

            _logger.LogInformation("Successfully aggregated course data for ID {CourseId}.", courseId);
            return dto;
        }

        
        private async Task<CourseReviewDto?> GetCourseReviewSummaryAsync(int courseId, List<string> warnings)
        {
            try
            {
                var data = await _discussionClient.GetReviewSummaryByCourseIdAsync(courseId); 
                
                if (data is null) warnings.Add($"Discussion service returned null for review summary for Course ID {courseId}.");
                return data;
            }
            catch (HttpRequestException ex)
            {
                warnings.Add($"Discussion service is unavailable for Course ID {courseId}. Error: {ex.Message}");
                _logger.LogError(ex, "Failed to fetch review summary for Course ID {CourseId} due to HTTP error.", courseId);
                return null;
            }
            catch (Exception ex)
            {
                 warnings.Add($"Unexpected error fetching review summary for Course ID {courseId}. Error: {ex.Message}");
                _logger.LogError(ex, "Unexpected error fetching review summary for Course ID {CourseId}.", courseId);
                return null;
            }
        }
    }