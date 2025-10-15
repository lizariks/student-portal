using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Services;


    public class CourseReviewService : ICourseReviewService
    {
        private readonly ICourseReviewRepository _repository;

        public CourseReviewService(ICourseReviewRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<CourseReview> AddReviewAsync(Guid targetId, TargetType targetType, UserInfo reviewer, int ratingValue, string comment)
        {
            var existingReviews = await _repository.GetByTargetAsync(targetId, targetType);
            bool alreadyReviewed = existingReviews.Any(r => r.Reviewer.UserId == reviewer.UserId);

            if (alreadyReviewed)
                throw new InvalidOperationException("User has already reviewed this target.");

            var review = new CourseReview(targetId, targetType, reviewer, new RatingValue(ratingValue), comment);
            await _repository.AddAsync(review);

            return review;
        }

        public async Task<CourseReview> UpdateReviewAsync(Guid reviewId, int newRatingValue, string newComment)
        {
            var review = await _repository.GetByIdAsync(reviewId)
                ?? throw new NotFoundException($"Review with ID {reviewId} not found.");

            review.UpdateRating(newRatingValue);
            review.UpdateComment(newComment);

            await _repository.UpdateAsync(review);
            return review;
        }

        public async Task DeleteReviewAsync(Guid reviewId)
        {
            var review = await _repository.GetByIdAsync(reviewId);
            if (review == null)
                throw new NotFoundException($"Review with ID {reviewId} not found.");

            await _repository.DeleteAsync(reviewId);
        }

        public async Task<CourseReview?> GetReviewByIdAsync(Guid reviewId)
        {
            return await _repository.GetByIdAsync(reviewId);
        }

        public async Task<IEnumerable<CourseReview>> GetReviewsByTargetAsync(Guid targetId, TargetType targetType)
        {
            return await _repository.GetByTargetAsync(targetId, targetType);
        }

        public async Task<double> GetAverageRatingAsync(Guid targetId, TargetType targetType)
        {
            var reviews = await _repository.GetByTargetAsync(targetId, targetType);
            if (!reviews.Any()) return 0.0;

            return reviews.Average(r => r.Rating.Value);
        }
    }
