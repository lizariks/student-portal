namespace StudentPortal.DiscussionService.Domain.Interfaces.Repositories;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
public interface IDiscussionThreadRepository : IMongoRepository<DiscussionThread>
{
    Task<IEnumerable<DiscussionThread>> GetByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken);
    Task<IEnumerable<DiscussionThread>> SearchByTitleAsync(string searchText, CancellationToken cancellationToken);
    Task<IEnumerable<DiscussionThread>> GetClosedThreadsAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken);
    Task<long> GetThreadCountByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken);
}