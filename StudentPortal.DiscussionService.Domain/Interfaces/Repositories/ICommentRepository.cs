namespace StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Entities;
public interface ICommentRepository : IMongoRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken);
    Task<long> GetThreadCommentCountAsync(Guid threadId, CancellationToken cancellationToken);
}