using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

public interface ICommentService
{
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken);
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken);
}