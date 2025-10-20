namespace StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.Parameters;
public interface ICourseReviewRepository : IMongoRepository<CourseReview>
{
    Task<PagedList<CourseReview>> GetCourseReviewsAsync(CourseReviewParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<CourseReview>> GetByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken);
    Task<IEnumerable<CourseReview>> GetByReviewerAsync(Guid reviewerId, CancellationToken cancellationToken);
    Task<double> GetAverageRatingAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken);
    Task<CourseReview?> GetByReviewerAndTargetAsync(Guid reviewerId, Guid targetId, TargetType targetType, CancellationToken cancellationToken);
}
