using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories;

public class CommentRepository : MongoRepository<Comment>, ICommentRepository
{
    private readonly IDiscussionThreadRepository _threadRepository;

    public CommentRepository(IMongoDatabase database, IDiscussionThreadRepository threadRepository) 
        : base(database, "comments")
    {
        _threadRepository = threadRepository;
    }

    public async Task<IEnumerable<Comment>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            return Enumerable.Empty<Comment>();

        return thread.Comments ?? Enumerable.Empty<Comment>();
    }

    public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken)
    {
        var filter = Builders<Comment>.Filter.Eq("author.userId", authorId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken)
    {
        var filter = Builders<Comment>.Filter.Regex(c => c.Content, new MongoDB.Bson.BsonRegularExpression(keyword, "i"));
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<long> GetThreadCommentCountAsync(Guid threadId, CancellationToken cancellationToken)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            return 0;

        return thread.Comments?.Count() ?? 0;
    }
}