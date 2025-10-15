using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ReopenDiscussionThread;
    public class ReopenDiscussionThreadCommandHandler : ICommandHandler<ReopenDiscussionThreadCommand, DiscussionThread>
    {
        private readonly IDiscussionThreadService _discussionThreadService;

        public ReopenDiscussionThreadCommandHandler(IDiscussionThreadService discussionThreadService)
        {
            _discussionThreadService = discussionThreadService;
        }

        public async Task<DiscussionThread> Handle(ReopenDiscussionThreadCommand request, CancellationToken cancellationToken)
        {
            var thread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            if (thread == null)
                throw new InvalidOperationException($"Discussion thread with ID '{request.ThreadId}' not found.");

            await _discussionThreadService.ReopenThreadAsync(request.ThreadId, request.Actor);

            var updatedThread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
            return updatedThread!;
        }
    }