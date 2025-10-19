using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories;

public class DiscussionThreadRepository : MongoRepository<DiscussionThread>, IDiscussionThreadRepository
{
    public DiscussionThreadRepository(IMongoDatabase database) 
        : base(database, "discussion-threads")
    {
    }

    public async Task<IEnumerable<DiscussionThread>> GetByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<DiscussionThread>.Filter.And(
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType)
        );
        
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> SearchByTitleAsync(string searchText, CancellationToken cancellationToken)
    {
        var filter = Builders<DiscussionThread>.Filter.Regex(
            t => t.Title, 
            new MongoDB.Bson.BsonRegularExpression(searchText, "i")
        );
        
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> GetClosedThreadsAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<DiscussionThread>.Filter.And(
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType),
            Builders<DiscussionThread>.Filter.Eq(t => t.IsClosed, true)
        );
        
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<long> GetThreadCountByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken)
    {
        var filter = Builders<DiscussionThread>.Filter.And(
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
            Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType)
        );
        
        return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}