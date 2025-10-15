using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Interfaces;
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

        public async Task<DiscussionThread> CreateThreadAsync(Guid targetId, TargetType targetType, string title, UserInfo createdBy)
        {
            if (targetId == Guid.Empty)
                throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));

            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidContentException();

            var thread = new DiscussionThread(targetId, targetType, title.Trim(), createdBy);

            await _threadRepository.AddAsync(thread);
            return thread;
        }

        public async Task CloseThreadAsync(Guid threadId, UserInfo actor)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                throw new NotFoundException($"Thread with Id '{threadId}' not found.");

            thread.Close(actor);
            await _threadRepository.UpdateAsync(thread);
        }

        public async Task ReopenThreadAsync(Guid threadId, UserInfo actor)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                throw new NotFoundException($"Thread with Id '{threadId}' not found.");

            thread.Reopen(actor);
            await _threadRepository.UpdateAsync(thread);
        }

        public async Task AddCommentAsync(Guid threadId, Comment comment)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                throw new NotFoundException($"Thread with Id '{threadId}' not found.");

            thread.AddComment(comment);
            await _threadRepository.UpdateAsync(thread);
        }

        public async Task EditCommentAsync(Guid threadId, Guid commentId, string newContent, UserInfo actor)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                throw new NotFoundException($"Thread with Id '{threadId}' not found.");

            var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
                throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");

            comment.Edit(newContent, actor);
            await _threadRepository.UpdateAsync(thread);
        }

        public async Task ResolveCommentAsync(Guid threadId, Guid commentId, UserInfo actor)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                throw new NotFoundException($"Thread with Id '{threadId}' not found.");

            var comment = thread.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
                throw new NotFoundException($"Comment with Id '{commentId}' not found in thread '{threadId}'.");

            comment.MarkAsResolved(actor);
            await _threadRepository.UpdateAsync(thread);
        }

        public async Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId)
        {
            return await _threadRepository.GetByIdAsync(threadId);
        }

        public async Task<IEnumerable<DiscussionThread>> GetThreadsByTargetAsync(Guid targetId, TargetType targetType)
        {
            return await _threadRepository.GetByTargetAsync(targetId, targetType);
        }

        public async Task<IEnumerable<DiscussionThread>> SearchThreadsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<DiscussionThread>();

            return await _threadRepository.SearchByTitleAsync(searchText.Trim());
        }
    }
