namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface ICourseReviewService
    {
        
        Task<CourseReview> AddReviewAsync(Guid targetId, TargetType targetType, UserInfo reviewer, int ratingValue,
            string comment, CancellationToken cancellationToken = default);

        Task<CourseReview> UpdateReviewAsync(Guid reviewId, int newRatingValue, string newComment,
            CancellationToken cancellationToken = default);

        Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

        Task<CourseReview?> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

        Task<IEnumerable<CourseReview>> GetReviewsByTargetAsync(Guid targetId, TargetType targetType,
            CancellationToken cancellationToken = default);

        Task<double> GetAverageRatingAsync(Guid targetId, TargetType targetType,
            CancellationToken cancellationToken = default);
    }
