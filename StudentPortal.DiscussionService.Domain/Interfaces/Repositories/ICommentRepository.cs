namespace StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.Parameters;
public interface ICommentRepository : IMongoRepository<Comment>
{
    Task<PagedList<Comment>> GetCommentsAsync(CommentParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Comment>> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken);
    Task<long> GetThreadCommentCountAsync(string threadId, CancellationToken cancellationToken);
}