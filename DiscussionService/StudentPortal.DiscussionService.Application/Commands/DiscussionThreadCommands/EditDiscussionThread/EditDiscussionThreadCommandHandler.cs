using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;
    public class EditThreadCommentCommandHandler : ICommandHandler<EditDiscussionThreadCommentCommand, DiscussionThread>
    {
        private readonly IDiscussionThreadService _discussionThreadService;

        public EditThreadCommentCommandHandler(IDiscussionThreadService discussionThreadService)
        {
            _discussionThreadService = discussionThreadService;
        }

        public async Task<DiscussionThread> Handle(EditDiscussionThreadCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NewContent))
                throw new ArgumentException("New comment content cannot be empty.", nameof(request.NewContent));

            var thread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            if (thread == null)
                throw new InvalidOperationException($"Thread with ID '{request.ThreadId}' not found.");

            await _discussionThreadService.EditCommentAsync(
                request.ThreadId,
                request.CommentId,
                request.NewContent,
                request.Actor
            );

            var updatedThread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            return updatedThread!;
        }
    }