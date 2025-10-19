using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories;

public class CourseReviewRepository : MongoRepository<CourseReview>, ICourseReviewRepository
{
    public CourseReviewRepository(IMongoDatabase database) 
        : base(database, "course-reviews")
    {
    }

    public async Task<IEnumerable<CourseReview>> GetByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<CourseReview>.Filter.And(
            Builders<CourseReview>.Filter.Eq(r => r.TargetId, targetId),
            Builders<CourseReview>.Filter.Eq(r => r.TargetType, targetType)
        );
        
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CourseReview>> GetByReviewerAsync(Guid reviewerId, CancellationToken cancellationToken)
    {
        var filter = Builders<CourseReview>.Filter.Eq("reviewer.userId", reviewerId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageRatingAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<CourseReview>.Filter.And(
            Builders<CourseReview>.Filter.Eq(r => r.TargetId, targetId),
            Builders<CourseReview>.Filter.Eq(r => r.TargetType, targetType)
        );

        var reviews = await _collection.Find(filter).ToListAsync(cancellationToken);
        
        if (!reviews.Any())
            return 0.0;

        return reviews.Average(r => r.Rating.Value);
    }

    public async Task<CourseReview?> GetByReviewerAndTargetAsync(Guid reviewerId, Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<CourseReview>.Filter.And(
            Builders<CourseReview>.Filter.Eq("reviewer.userId", reviewerId),
            Builders<CourseReview>.Filter.Eq(r => r.TargetId, targetId),
            Builders<CourseReview>.Filter.Eq(r => r.TargetType, targetType)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}