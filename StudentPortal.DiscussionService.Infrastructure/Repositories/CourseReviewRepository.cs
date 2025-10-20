using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories;

public class CourseReviewRepository : MongoRepository<CourseReview>, ICourseReviewRepository
{
    public CourseReviewRepository(MongoDbContext context, IClientSessionHandle? session = null)
        : base(context, session)
    {
    }
    
    public async Task<PagedList<CourseReview>> GetCourseReviewsAsync(
            CourseReviewParameters parameters,
            CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<CourseReview>.Filter;
            var filter = FilterDefinition<CourseReview>.Empty;

            if (parameters.TargetId.HasValue)
                filter &= filterBuilder.Eq(r => r.TargetId, parameters.TargetId.Value);

            if (parameters.TargetType.HasValue)
                filter &= filterBuilder.Eq(r => r.TargetType, parameters.TargetType.Value);
            if (parameters.CreatedFrom.HasValue)
                filter &= filterBuilder.Gte(r => r.CreatedAt, parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                filter &= filterBuilder.Lte(r => r.CreatedAt, parameters.CreatedTo.Value);

            var sortHelper = new MongoSortHelper<CourseReview>();
            var sortDefinition = sortHelper.ApplySort(parameters.OrderBy ?? parameters.SortBy);

            if (parameters.SortDescending)
                sortDefinition = Builders<CourseReview>.Sort.Combine(sortDefinition, Builders<CourseReview>.Sort.Descending(parameters.SortBy));

            var findFluent = _collection
                .Find(filter)
                .Sort(sortDefinition);

            return await PagedList<CourseReview>.ToPagedListAsync(
                findFluent,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken
            );
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