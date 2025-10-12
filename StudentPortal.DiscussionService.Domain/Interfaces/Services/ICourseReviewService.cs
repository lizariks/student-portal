namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface ICourseReviewService
    {
        Task<CourseReview> AddReviewAsync(Guid targetId, TargetType targetType, UserInfo reviewer, int ratingValue, string comment);
        Task<CourseReview> UpdateReviewAsync(Guid reviewId, int newRatingValue, string newComment);
        Task DeleteReviewAsync(Guid reviewId);

        Task<CourseReview?> GetReviewByIdAsync(Guid reviewId);
        Task<IEnumerable<CourseReview>> GetReviewsByTargetAsync(Guid targetId, TargetType targetType);
        Task<double> GetAverageRatingAsync(Guid targetId, TargetType targetType);
    }
