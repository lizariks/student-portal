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

    public async Task<DiscussionThread> CreateThreadAsync(string targetId, TargetType targetType, string title, UserInfo createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));

        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidContentException();

        var thread = new DiscussionThread(targetId, targetType, title.Trim(), createdBy);

        await _threadRepository.AddAsync(thread, cancellationToken);
        return thread;
    }

    public async Task CloseThreadAsync(string threadId, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.Close(actor);
        await _threadRepository.UpdateAsync(thread, cancellationToken);
    }

    public async Task ReopenThreadAsync(string threadId, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.Reopen(actor);
        await _threadRepository.UpdateAsync(thread, cancellationToken);
    }

    public async Task AddCommentAsync(string threadId, Comment comment, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        thread.AddComment(comment);
        await _threadRepository.UpdateAsync(thread, cancellationToken);
    }

    public async Task EditCommentAsync(string threadId, string commentId, string newContent, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
            throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");

        comment.Edit(newContent, actor);
        thread.MarkUpdated();

        await _threadRepository.UpdateAsync(thread, cancellationToken);
    }

    public async Task ResolveCommentAsync(string threadId, string commentId, UserInfo actor, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread == null)
            throw new NotFoundException($"Thread with Id '{threadId}' not found.");

        var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
            throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");

        comment.MarkAsResolved(actor);
        thread.MarkUpdated();

        await _threadRepository.UpdateAsync(thread, cancellationToken);
    }

    public async Task<DiscussionThread?> GetThreadByIdAsync(string threadId, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetByIdAsync(threadId, cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(string targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetByTargetAsync(targetId, targetType, cancellationToken);
    }

    public async Task<IEnumerable<DiscussionThread>> GetClosedThreadsAsync(string targetId, TargetType targetType, CancellationToken cancellationToken = default)
    {
        return await _threadRepository.GetClosedThreadsAsync(targetId, targetType, cancellationToken);
    }

    public async Task<long> GetThreadCountAsync(string targetId, TargetType targetType, CancellationToken cancellationToken = default)
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
