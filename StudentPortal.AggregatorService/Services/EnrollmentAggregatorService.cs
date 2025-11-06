using StudentPortal.AggregatorService.Clients;
using StudentPortal.AggregatorService.DTOs.Aggregated; 
using StudentPortal.AggregatorService.DTOs.Enrollment; 
using StudentPortal.AggregatorService.DTOs.CourseCatalog; 
using StudentPortal.AggregatorService.DTOs.Discussion; // Ваш CourseReviewDto

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StudentPortal.AggregatorService.Services;
    /// <summary>
    /// Сервіс, який агрегує дані про запис студента (Enrollment) з усіх трьох мікросервісів.
    /// </summary>
    public class EnrollmentAggregatorService
    {
        private readonly EnrollmentClient _enrollmentClient;
        private readonly CourseCatalogClient _catalogClient;
        private readonly DiscussionClient _discussionClient;
        private readonly ILogger<EnrollmentAggregatorService> _logger;

        public EnrollmentAggregatorService(
            EnrollmentClient enrollmentClient,
            CourseCatalogClient catalogClient,
            DiscussionClient discussionClient,
            ILogger<EnrollmentAggregatorService> logger)
        {
            _enrollmentClient = enrollmentClient;
            _catalogClient = catalogClient;
            _discussionClient = discussionClient;
            _logger = logger;
        }

        /// <summary>
        /// Отримує агреговані деталі запису студента за ID.
        /// </summary>
        public async Task<AggregatedEnrollmentDto?> GetAggregatedEnrollmentByIdAsync(int enrollmentId, CancellationToken ct)
        {
            // 1. Отримання базових даних Enrollment (Критична залежність)
            EnrollmentDto? enrollment = null;
            try
            {
                enrollment = await _enrollmentClient.GetEnrollmentByIdAsync(enrollmentId);
                if (enrollment is null)
                {
                    _logger.LogWarning("Enrollment with ID {EnrollmentId} not found.", enrollmentId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch base enrollment data for ID {EnrollmentId}. Tracing is critical.", enrollmentId);
                throw; 
            }

            var warnings = new List<string>();
            var courseId = enrollment.CourseId;

            // 2. Паралельні запити до Catalog та Discussion Services
            var catalogTask = GetCourseCatalogDataAsync(courseId, warnings, ct);
            var reviewsTask = GetCourseReviewSummaryAsync(courseId, warnings, ct); // Викликаємо новий допоміжний метод

            await Task.WhenAll(catalogTask, reviewsTask);

            // 3. Зведення даних
            var catalogData = await catalogTask;
            var reviewSummary = await reviewsTask;
            
            // Перевірка консистентності даних
            if (catalogData != null && catalogData.Id != courseId)
            {
                 warnings.Add($"Consistency check failed: Catalog returned data for Course ID {catalogData.Id}, expected {courseId}.");
                 _logger.LogError("Inconsistent CourseId received: Expected {ExpectedId}, Got {ReceivedId}.", courseId, catalogData.Id);
            }

            var dto = new AggregatedEnrollmentDto
            {
                // Enrollment Data
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CurrentStatus = enrollment.Status,
                EnrolledAt = enrollment.EnrolledAt,
                
                // History
                StatusHistory = enrollment.StatusHistories.Select(h => new EnrollmentStatusHistoryDto 
                {
                    NewStatus = h.NewStatus, 
                    ChangedAt = h.ChangedAt 
                }).ToList(),

                // Course Data (Catalog)
                CourseId = courseId,
                CourseTitle = catalogData?.Title ?? "N/A (Catalog Unavailable)",
                CourseCode = catalogData?.Code ?? "N/A",
                InstructorId = catalogData?.InstructorId,
                
                // Review Data (Discussion)
                AverageRating = reviewSummary?.AverageRating,
                TotalReviews = reviewSummary?.TotalReviews ?? 0,
            };

            _logger.LogInformation("Successfully aggregated enrollment data for ID {EnrollmentId}.", enrollmentId);
            return dto;
        }

        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Даних Каталогу ---

        private async Task<CourseDto?> GetCourseCatalogDataAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
            try
            {
                var data = await _catalogClient.GetCourseByIdAsync(courseId);
                if (data is null) warnings.Add("Course Catalog service returned null.");
                return data;
            }
            catch (HttpRequestException ex)
            {
                warnings.Add($"Course Catalog service is unavailable. Error: {ex.Message}");
                _logger.LogError(ex, "Failed to fetch catalog data for Course ID {CourseId} due to HTTP error.", courseId);
                return null;
            }
            catch (Exception ex)
            {
                 warnings.Add($"Unexpected error fetching catalog data. Error: {ex.Message}");
                _logger.LogError(ex, "Unexpected error fetching catalog data for Course ID {CourseId}.", courseId);
                return null;
            }
        }
        
        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Агрегації Відгуків ---
        
        private async Task<CourseReviewDto?> GetCourseReviewSummaryAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
            try
            {
                // Спеціалізований клієнтський метод, який ми додали.
                // Припускаємо, що DiscussionClient тепер має метод GetReviewSummaryByCourseIdAsync.
                var data = await _discussionClient.GetReviewSummaryByCourseIdAsync(courseId); 
                
                if (data is null) warnings.Add("Discussion service returned null for review summary.");
                return data;
            }
             catch (HttpRequestException ex)
            {
                warnings.Add($"Discussion service is unavailable. Error: {ex.Message}");
                _logger.LogError(ex, "Failed to fetch review summary for Course ID {CourseId} due to HTTP error.", courseId);
                return null;
            }
            catch (Exception ex)
            {
                 warnings.Add($"Unexpected error fetching review summary. Error: {ex.Message}");
                _logger.LogError(ex, "Unexpected error fetching review summary for Course ID {CourseId}.", courseId);
                return null;
            }
        }
    }