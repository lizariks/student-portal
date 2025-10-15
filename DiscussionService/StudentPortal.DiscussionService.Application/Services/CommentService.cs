using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Services;

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        
        public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            await _commentRepository.AddAsync(comment, cancellationToken);
        }
        
        public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
                throw new NotFoundException($"Comment with ID '{id}' was not found.");

            return comment;
        }
        
        public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            var existing = await _commentRepository.GetByIdAsync(comment.Id, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Comment with ID '{comment.Id}' was not found.");

            await _commentRepository.UpdateAsync(comment, cancellationToken);
        }
        
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var existing = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Comment with ID '{id}' was not found.");

            await _commentRepository.DeleteAsync(id, cancellationToken);
        }
        
        public async Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("Search keyword cannot be empty.", nameof(keyword));

            return await _commentRepository.SearchByContentAsync(keyword.Trim(), cancellationToken);
        }
    }
