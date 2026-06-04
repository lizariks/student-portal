using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;
    public class ResolveDiscussionThreadCommandHandler : ICommandHandler<ResolveDiscussionThreadCommand, DiscussionThread>
    {
        private readonly IDiscussionThreadService _discussionThreadService;

        public ResolveDiscussionThreadCommandHandler(IDiscussionThreadService discussionThreadService)
        {
            _discussionThreadService = discussionThreadService;
        }

        public async Task<DiscussionThread> Handle(ResolveDiscussionThreadCommand request, CancellationToken cancellationToken)
        {
            var thread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            if (thread == null)
                throw new InvalidOperationException($"Thread with ID '{request.ThreadId}' not found.");

            await _discussionThreadService.ResolveCommentAsync(
                request.ThreadId,
                request.CommentId,
                request.Actor
            );

            var updatedThread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            return updatedThread!;
        }
    }