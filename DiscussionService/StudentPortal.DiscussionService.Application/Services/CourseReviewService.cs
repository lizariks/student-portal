using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.Parameters;

namespace StudentPortal.DiscussionService.Application.Services;

public class CourseReviewService : ICourseReviewService
{
    private readonly ICourseReviewRepository _repository;

    public CourseReviewService(ICourseReviewRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    
    public async Task<PagedList<CourseReview>> GetCourseReviewsAsync(CourseReviewParameters parameters, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCourseReviewsAsync(parameters, cancellationToken);
    }

    public async Task<CourseReview> AddReviewAsync(string targetId, TargetType targetType, UserInfo reviewer, int ratingValue, string comment, CancellationToken cancellationToken = default)
    {
        var existingReview = await _repository.GetByReviewerAndTargetAsync(reviewer.UserId, targetId, targetType, cancellationToken);

        if (existingReview != null)
            throw new InvalidOperationException("User has already reviewed this target.");

        var review = new CourseReview(targetId, targetType, reviewer, new RatingValue(ratingValue), comment);
        await _repository.AddAsync(review, cancellationToken);

        return review;
    }

    public async Task<CourseReview> UpdateReviewAsync(string reviewId, int newRatingValue, string newComment, CancellationToken cancellationToken = default)
    {
        var review = await _repository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new NotFoundException($"Review with ID {reviewId} not found.");

        review.UpdateRating(newRatingValue);
        review.UpdateComment(newComment);

        await _repository.UpdateAsync(review, cancellationToken);
        return review;
    }

    public async Task DeleteReviewAsync(string reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _repository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
            throw new NotFoundException($"Review with ID {reviewId} not found.");

        await _repository.DeleteAsync(reviewId, cancellationToken);
    }

    public async Task<CourseReview?> GetReviewByIdAsync(string reviewId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(reviewId, cancellationToken);
    }

    public async Task<IEnumerable<CourseReview>> GetReviewsByTargetAsync(string targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByTargetAsync(targetId, targetType, cancellationToken);
    }
    
    public async Task<double> GetAverageRatingAsync(string targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _repository.GetAverageRatingAsync(targetId, targetType, cancellationToken);
    }
}
