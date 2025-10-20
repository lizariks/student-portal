using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.Parameters;


namespace StudentPortal.DiscussionService.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }
    
    public async Task<PagedList<Comment>> GetCommentsAsync (CommentParameters parameters, CancellationToken cancellationToken=default)
    {
        return await _commentRepository.GetCommentsAsync(parameters, cancellationToken);
    }
    
    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken)
    {
        if (comment == null)
            throw new ArgumentNullException(nameof(comment));

        await _commentRepository.AddAsync(comment);
    }
    
    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
            throw new NotFoundException($"Comment with ID '{id}' was not found.");

        return comment;
    }
    
    public async Task<IEnumerable<Comment>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetByThreadIdAsync(threadId, cancellationToken);
    }
    
    public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetByAuthorIdAsync(authorId, cancellationToken);
    }
    
    public async Task<long> GetThreadCommentCountAsync(Guid threadId, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetThreadCommentCountAsync(threadId, cancellationToken);
    }
    
    public async Task UpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _commentRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Comment with ID '{id}' was not found.");

        await _commentRepository.UpdateAsync(existing);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _commentRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Comment with ID '{id}' was not found.");

        await _commentRepository.DeleteAsync(id);
    }
    
    public async Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Search keyword cannot be empty.", nameof(keyword));

        return await _commentRepository.SearchByContentAsync(keyword.Trim(), cancellationToken);
    }
}