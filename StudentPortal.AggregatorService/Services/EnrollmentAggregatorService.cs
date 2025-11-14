using StudentPortal.AggregatorService.Clients;
using StudentPortal.AggregatorService.DTOs.Aggregated; 
using StudentPortal.AggregatorService.DTOs.Enrollment; 
using StudentPortal.AggregatorService.DTOs.CourseCatalog; 
using StudentPortal.AggregatorService.DTOs.Discussion; 

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

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
        /// Отримує список усіх агрегованих записів про зарахування.
        /// </summary>
        public async Task<List<AggregatedEnrollmentDto>> GetAllEnrollmentsAggregatedAsync(CancellationToken ct = default)
        {
            List<EnrollmentDto>? enrollments;
            try
            {
                // 1. Отримання базових даних усіх записів (Критична залежність)
                enrollments = await _enrollmentClient.GetAllEnrollmentsAsync();
                if (enrollments is null || !enrollments.Any())
                {
                    _logger.LogWarning("No enrollments found.");
                    return new List<AggregatedEnrollmentDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch base enrollment data. Aborting aggregation.");
                throw; 
            }

            var warnings = new List<string>();
            var uniqueCourseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
            
            // 2. Паралельні запити для унікальних курсів
            var courseDataTasks = uniqueCourseIds.ToDictionary(
                id => id,
                id => GetAggregatedCourseDataAsync(id, warnings, ct)
            );

            await Task.WhenAll(courseDataTasks.Values);

            // 3. Комбінування даних
            var aggregatedEnrollments = new List<AggregatedEnrollmentDto>();

            foreach (var enrollment in enrollments)
            {
                var courseData = courseDataTasks.GetValueOrDefault(enrollment.CourseId)?.Result;
                
                aggregatedEnrollments.Add(new AggregatedEnrollmentDto
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

                    // Course Data (Catalog + Discussion)
                    CourseId = enrollment.CourseId,
                    CourseTitle = courseData?.Title ?? "N/A (Catalog Unavailable)",
                    CourseCode = courseData?.Code ?? "N/A",
                    InstructorId = courseData?.InstructorId,
                    AverageRating = courseData?.AverageRating,
                    TotalReviews = courseData?.TotalReviews ?? 0,
                });
            }
            
            if (warnings.Any())
            {
                _logger.LogWarning("Aggregation completed with warnings: {Warnings}", string.Join("; ", warnings));
            }
            _logger.LogInformation("Successfully aggregated {Count} enrollments.", aggregatedEnrollments.Count);
            return aggregatedEnrollments;
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

            // 2. Паралельний запит агрегованих даних курсу
            var courseAggregatedDataTask = GetAggregatedCourseDataAsync(courseId, warnings, ct);

            await Task.WhenAll(courseAggregatedDataTask);

            // 3. Зведення даних
            var courseData = await courseAggregatedDataTask;
            
            // Перевірка консистентності даних
            if (courseData != null && courseData.Id != courseId)
            {
                 warnings.Add($"Consistency check failed: Aggregated course data returned data for Course ID {courseData.Id}, expected {courseId}.");
                 _logger.LogError("Inconsistent CourseId received: Expected {ExpectedId}, Got {ReceivedId}.", courseId, courseData.Id);
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

                // Course Data (Catalog + Discussion)
                CourseId = courseId,
                CourseTitle = courseData?.Title ?? "N/A (Catalog Unavailable)",
                CourseCode = courseData?.Code ?? "N/A",
                InstructorId = courseData?.InstructorId,
                AverageRating = courseData?.AverageRating,
                TotalReviews = courseData?.TotalReviews ?? 0,
            };

            _logger.LogInformation("Successfully aggregated enrollment data for ID {EnrollmentId}.", enrollmentId);
            return dto;
        }

        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Агрегованих Даних Курсу ---

        private async Task<AggregatedCourseCourseDetailsDto?> GetAggregatedCourseDataAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
             // Цей DTO має містити поля CourseDto + CourseReviewDto. 
             // Припускаємо, що він існує, або ми можемо створити його ad-hoc.
             // Для спрощення використовуємо внутрішню структуру.
             
            var catalogTask = GetCourseCatalogDataAsync(courseId, warnings, ct);
            var reviewsTask = GetCourseReviewSummaryAsync(courseId, warnings, ct);

            await Task.WhenAll(catalogTask, reviewsTask);
            
            var catalogData = await catalogTask;
            var reviewSummary = await reviewsTask;

            if (catalogData == null) return null;

            return new AggregatedCourseCourseDetailsDto
            {
                Id = catalogData.Id,
                Title = catalogData.Title,
                Code = catalogData.Code,
                // Поля з Discussion
                AverageRating = reviewSummary?.AverageRating,
                TotalReviews = reviewSummary?.TotalReviews ?? 0
            };
        }

        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Даних Каталогу (Залишається без змін) ---

        private async Task<CourseDto?> GetCourseCatalogDataAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
            try
            {
                // Тут ми не використовуємо CancellationToken, оскільки HttpClient.GetFromJsonAsync
                // у старіших версіях .NET Core/Framework може його не підтримувати без 
                // додаткової обгортки (хоча у сучасних версіях це працює).
                var data = await _catalogClient.GetCourseByIdAsync(courseId);
                if (data is null) warnings.Add($"Course Catalog service returned null for Course ID {courseId}.");
                return data;
            }
            catch (HttpRequestException ex)
            {
                warnings.Add($"Course Catalog service is unavailable for Course ID {courseId}. Error: {ex.Message}");
                _logger.LogError(ex, "Failed to fetch catalog data for Course ID {CourseId} due to HTTP error.", courseId);
                return null;
            }
            catch (Exception ex)
            {
                 warnings.Add($"Unexpected error fetching catalog data for Course ID {courseId}. Error: {ex.Message}");
                _logger.LogError(ex, "Unexpected error fetching catalog data for Course ID {CourseId}.", courseId);
                return null;
            }
        }
        
        // --- ДОПОМІЖНИЙ МЕТОД: Отримання Агрегації Відгуків (Залишається без змін) ---
        
        private async Task<CourseReviewDto?> GetCourseReviewSummaryAsync(int courseId, List<string> warnings, CancellationToken ct)
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

        // Припускаємо, що цей DTO існує для зручності передачі агрегованих даних курсу
        private class AggregatedCourseCourseDetailsDto 
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Code { get; set; }
            public int InstructorId { get; set; }
            public double? AverageRating { get; set; }
            public int TotalReviews { get; set; }
        }
    }