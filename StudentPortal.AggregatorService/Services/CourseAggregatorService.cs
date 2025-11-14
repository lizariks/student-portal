using StudentPortal.AggregatorService.Clients;
using StudentPortal.AggregatorService.DTOs.Aggregated; // Припускаємо, що тут AggregatedCourseDto
using StudentPortal.AggregatorService.DTOs.CourseCatalog; // Ваш CourseDto
using StudentPortal.AggregatorService.DTOs.Discussion; // Ваш CourseReviewDto


namespace StudentPortal.AggregatorService.Services;
    /// <summary>
    /// Сервіс, який агрегує інформацію про курс, виключаючи деталі структури (модулі/уроки).
    /// </summary>
    public class CourseAggregatorService
    {
        private readonly CourseCatalogClient _catalogClient;
        private readonly DiscussionClient _discussionClient;
        private readonly ILogger<CourseAggregatorService> _logger;

        public CourseAggregatorService(
            CourseCatalogClient catalogClient,
            DiscussionClient discussionClient,
            ILogger<CourseAggregatorService> logger)
        {
            _catalogClient = catalogClient;
            _discussionClient = discussionClient;
            _logger = logger;
        }

        /// <summary>
        /// Отримує список усіх агрегованих курсів (деталі та відгуки).
        /// </summary>
        public async Task<List<AggregatedCourseDto>> GetAllCoursesAggregatedAsync()
        {
            List<CourseDto>? courses;
            try
            {
                // 1. Отримання базових даних усіх курсів (Критична залежність)
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

            // 2. Паралельний запит зведення відгуків для всіх курсів
            var reviewTasks = courses.Select(c => GetCourseReviewSummaryAsync(c.Id, warnings)).ToList();

            await Task.WhenAll(reviewTasks);

            var reviewSummaries = reviewTasks.Select(t => t.Result).ToList();

            // 3. Мапінг на фінальну DTO
            var aggregatedCourses = courses.Select((course, index) =>
            {
                var reviewSummary = reviewSummaries[index];
                
                return new AggregatedCourseDto
                {
                    // Course Data (Catalog - Тільки плоскі поля)
                    Id = course.Id,
                    Title = course.Title,
                    Code = course.Code,
                    Description = course.Description,
                    CreatedAt = course.CreatedAt,
                    InstructorId = course.InstructorId,
                    
                    // Review Data (Discussion)
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

        /// <summary>
        /// Отримує агреговану інформацію про курс (деталі та відгуки).
        /// </summary>
        public async Task<AggregatedCourseDto?> GetAggregatedCourseByIdAsync(int courseId)
        {
            CourseDto? course = null;
            try
            {
                // 1. Отримання базових даних курсу (Критична залежність)
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

            // 2. Паралельний запит до Discussion Service
            var reviewsTask = GetCourseReviewSummaryAsync(courseId, warnings);

            await Task.WhenAll(reviewsTask);

            var reviewSummary = await reviewsTask;

            // 3. Мапінг на фінальну DTO
            var dto = new AggregatedCourseDto
            {
                // Course Data (Catalog - Тільки плоскі поля)
                Id = course.Id,
                Title = course.Title,
                Code = course.Code,
                Description = course.Description,
                CreatedAt = course.CreatedAt,
                InstructorId = course.InstructorId,
                
                // Review Data (Discussion)
                AverageRating = reviewSummary?.AverageRating,
                TotalReviews = reviewSummary?.TotalReviews ?? 0,
                
                // Модулі та уроки тут ІГНОРУЮТЬСЯ
            };

            _logger.LogInformation("Successfully aggregated course data for ID {CourseId}.", courseId);
            return dto;
        }

        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Агрегації Відгуків (Залишається без змін) ---
        
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