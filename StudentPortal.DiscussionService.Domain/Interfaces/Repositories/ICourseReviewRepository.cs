namespace StudentPortal.DiscussionService.Domain.Interfaces;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
public interface ICourseReviewRepository
{
    Task<CourseReview?> GetByIdAsync(Guid id);
    Task<IEnumerable<CourseReview>> GetByTargetAsync(Guid targetId, TargetType targetType);
    Task<IEnumerable<CourseReview>> GetByReviewerAsync(Guid reviewerId);
    Task AddAsync(CourseReview review);
    Task UpdateAsync(CourseReview review); 
    Task DeleteAsync(Guid id);
    Task<double> GetAverageRatingAsync(Guid targetId); 
}