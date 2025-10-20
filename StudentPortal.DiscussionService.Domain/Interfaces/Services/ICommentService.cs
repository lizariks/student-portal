using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Common;
namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.Parameters;

public interface ICommentService
{
    Task<PagedList<Comment>> GetCommentsAsync(CommentParameters comment, CancellationToken cancellationToken);
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken);
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken);
}