using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Services; 
    public class DiscussionThreadService : IDiscussionThreadService
{
    private readonly IDiscussionThreadRepository _threadRepository;

    public DiscussionThreadService(IDiscussionThreadRepository threadRepository)
    {
        _threadRepository = threadRepository;
    }

    public async Task<DiscussionThread> CreateThreadAsync(Guid targetId, TargetType targetType, string title, UserInfo createdBy, CancellationToken cancellationToken = default)
    {
        if (targetId == Guid.Empty)
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));

        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidContentException();

        var thread = new DiscussionThread(targetId, targetType, title.Trim(), createdBy);

        await _threadRepository.AddAsync(thread);
        return thread;
    }

    public async Task CloseThreadAsync(Guid threadId, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.Close(actor);
        await _threadRepository.UpdateAsync(thread);
    }

    public async Task ReopenThreadAsync(Guid threadId, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.Reopen(actor);
        await _threadRepository.UpdateAsync(thread);
    }

    public async Task AddCommentAsync(Guid threadId, Comment comment, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.AddComment(comment);
        await _threadRepository.UpdateAsync(thread);
    }

    public async Task EditCommentAsync(
        Guid threadId,
        Guid commentId,
        string newContent,
        UserInfo actor,
        CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId.ToString());
        if (comment == null)
            throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");

        comment.Edit(newContent, actor);
        thread.MarkUpdated();

        await _threadRepository.UpdateAsync(thread);
    }


    public async Task ResolveCommentAsync(
        Guid threadId,
        Guid commentId,
        UserInfo actor,
        CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId.ToString());
        if (comment == null)
            throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");
        comment.MarkAsResolved(actor);

        thread.MarkUpdated();

        await _threadRepository.UpdateAsync(thread);
    }


    public async Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetByIdAsync(threadId);
    }

    public async Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetByTargetAsync(targetId, targetType, cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> GetClosedThreadsAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetClosedThreadsAsync(targetId, targetType, cancellationToken);
    }

    public async Task<long> GetThreadCountAsync(Guid targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetThreadCountByTargetAsync(targetId, targetType, cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> SearchThreadsAsync(string searchText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Enumerable.Empty<DiscussionThread>();

        return await _threadRepository.SearchByTitleAsync(searchText.Trim(), cancellationToken);
    }
}