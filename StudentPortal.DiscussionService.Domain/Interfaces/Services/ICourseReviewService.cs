namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface ICourseReviewService
    {
        
        Task<CourseReview> AddReviewAsync(string targetId, TargetType targetType, UserInfo reviewer, int ratingValue,
            string comment, CancellationToken cancellationToken = default);

        Task<CourseReview> UpdateReviewAsync(string reviewId, int newRatingValue, string newComment,
            CancellationToken cancellationToken = default);

        Task DeleteReviewAsync(string reviewId, CancellationToken cancellationToken = default);

        Task<CourseReview?> GetReviewByIdAsync(string reviewId, CancellationToken cancellationToken = default);

        Task<IEnumerable<CourseReview>> GetReviewsByTargetAsync(string targetId, TargetType targetType,
            CancellationToken cancellationToken = default);

        Task<double> GetAverageRatingAsync(string targetId, TargetType targetType,
            CancellationToken cancellationToken = default);
    }
