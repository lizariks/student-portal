namespace StudentPortal.DiscussionService.Domain.Interfaces.Services;

using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

    public interface IDiscussionThreadService
    {
        Task<DiscussionThread> CreateThreadAsync(string targetId, TargetType targetType, string title, UserInfo createdBy,CancellationToken cancellationToken = default);
        Task CloseThreadAsync(string threadId, UserInfo actor, CancellationToken cancellationToken = default);
        Task ReopenThreadAsync(string threadId, UserInfo actor,CancellationToken cancellationToken = default);
        Task AddCommentAsync(string threadId, Comment comment,CancellationToken cancellationToken = default);

        Task EditCommentAsync(string threadId, string commentId, string newContent, UserInfo actor,
            CancellationToken cancellationToken = default);

        Task DeleteCommentAsync(string threadId, string commentId, UserInfo actor,
            CancellationToken cancellationToken = default);

        Task ResolveCommentAsync(string threadId, string commentId, UserInfo actor,
            CancellationToken cancellationToken = default);
        Task<DiscussionThread?> GetThreadByIdAsync(string threadId,CancellationToken cancellationToken = default);
        Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(string targetId, TargetType targetType,CancellationToken cancellationToken = default);
        Task<IEnumerable<DiscussionThread>> SearchThreadsAsync(string searchText,CancellationToken cancellationToken = default);
    }
