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
    public class EnrollmentAggregatorService
    {
        private readonly EnrollmentGrpcClient _enrollmentClient;
        private readonly CourseCatalogGrpcClient _catalogClient;
        private readonly DiscussionGrpcClient _discussionClient;
        private readonly ILogger<EnrollmentAggregatorService> _logger;

        public EnrollmentAggregatorService(
            EnrollmentGrpcClient enrollmentClient,
            CourseCatalogGrpcClient catalogClient,
            DiscussionGrpcClient discussionClient,
            ILogger<EnrollmentAggregatorService> logger)
        {
            _enrollmentClient = enrollmentClient;
            _catalogClient = catalogClient;
            _discussionClient = discussionClient;
            _logger = logger;
        }
        public async Task<List<AggregatedEnrollmentDto>> GetAllEnrollmentsAggregatedAsync(CancellationToken ct = default)
        {
            List<EnrollmentDto>? enrollments;
            try
            {
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
            
            var courseDataTasks = uniqueCourseIds.ToDictionary(
                id => id,
                id => GetAggregatedCourseDataAsync(id, warnings, ct)
            );

            await Task.WhenAll(courseDataTasks.Values);

            var aggregatedEnrollments = new List<AggregatedEnrollmentDto>();

            foreach (var enrollment in enrollments)
            {
                var courseData = courseDataTasks.GetValueOrDefault(enrollment.CourseId)?.Result;
                
                aggregatedEnrollments.Add(new AggregatedEnrollmentDto
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    CurrentStatus = enrollment.Status,
                    EnrolledAt = enrollment.EnrolledAt,
                    
                    StatusHistory = enrollment.StatusHistories.Select(h => new EnrollmentStatusHistoryDto 
                    {
                        NewStatus = h.NewStatus, 
                        ChangedAt = h.ChangedAt 
                    }).ToList(),

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

        public async Task<AggregatedEnrollmentDto?> GetAggregatedEnrollmentByIdAsync(int enrollmentId, CancellationToken ct)
        {
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

            var courseAggregatedDataTask = GetAggregatedCourseDataAsync(courseId, warnings, ct);

            await Task.WhenAll(courseAggregatedDataTask);

            var courseData = await courseAggregatedDataTask;
            
            if (courseData != null && courseData.Id != courseId)
            {
                 warnings.Add($"Consistency check failed: Aggregated course data returned data for Course ID {courseData.Id}, expected {courseId}.");
                 _logger.LogError("Inconsistent CourseId received: Expected {ExpectedId}, Got {ReceivedId}.", courseId, courseData.Id);
            }

            var dto = new AggregatedEnrollmentDto
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CurrentStatus = enrollment.Status,
                EnrolledAt = enrollment.EnrolledAt,
                
                StatusHistory = enrollment.StatusHistories.Select(h => new EnrollmentStatusHistoryDto 
                {
                    NewStatus = h.NewStatus, 
                    ChangedAt = h.ChangedAt 
                }).ToList(),

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


        private async Task<AggregatedCourseCourseDetailsDto?> GetAggregatedCourseDataAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
             
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
                AverageRating = reviewSummary?.AverageRating,
                TotalReviews = reviewSummary?.TotalReviews ?? 0
            };
        }


        private async Task<CourseDto?> GetCourseCatalogDataAsync(int courseId, List<string> warnings, CancellationToken ct)
        {
            try
            {
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
        
    }