namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface IDiscussionThreadService
    {
        Task<DiscussionThread> CreateThreadAsync(Guid targetId, TargetType targetType, string title, UserInfo createdBy);
        Task CloseThreadAsync(Guid threadId, UserInfo actor);
        Task ReopenThreadAsync(Guid threadId, UserInfo actor);
        Task AddCommentAsync(Guid threadId, Comment comment);
        Task EditCommentAsync(Guid threadId, Guid commentId, string newContent, UserInfo actor);
        Task ResolveCommentAsync(Guid threadId, Guid commentId, UserInfo actor);
        Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId);
        Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(Guid targetId, TargetType targetType);
        Task<IEnumerable<DiscussionThread>> SearchThreadsAsync(string searchText);
    }
