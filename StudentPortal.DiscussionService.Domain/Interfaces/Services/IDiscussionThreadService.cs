namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface IDiscussionThreadService
    {
        Task<DiscussionThread> CreateThreadAsync(Guid targetId, TargetType targetType, string title, UserInfo createdBy,CancellationToken cancellationToken = default);
        Task CloseThreadAsync(Guid threadId, UserInfo actor, CancellationToken cancellationToken = default);
        Task ReopenThreadAsync(Guid threadId, UserInfo actor,CancellationToken cancellationToken = default);
        Task AddCommentAsync(Guid threadId, Comment comment,CancellationToken cancellationToken = default);
        Task EditCommentAsync(Guid threadId, Guid commentId, string newContent, UserInfo actor,CancellationToken cancellationToken = default);
        Task ResolveCommentAsync(Guid threadId, Guid commentId, UserInfo actor,CancellationToken cancellationToken = default);
        Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId,CancellationToken cancellationToken = default);
        Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(Guid targetId, TargetType targetType,CancellationToken cancellationToken = default);
        Task<IEnumerable<DiscussionThread>> SearchThreadsAsync(string searchText,CancellationToken cancellationToken = default);
    }
