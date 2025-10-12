namespace StudentPortal.DiscussionService.Domain.Interfaces;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Entities.Enums;
public interface IDiscussionThreadRepository
{
    Task<DiscussionThread?> GetByIdAsync(Guid id);
    Task<IEnumerable<DiscussionThread>> GetByTargetAsync(Guid targetId, TargetType targetType);
    Task<IEnumerable<DiscussionThread>> SearchByTitleAsync(string searchText);
    Task AddAsync(DiscussionThread thread);
    Task UpdateAsync(DiscussionThread thread); 
    Task DeleteAsync(Guid id);
}