using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;
    public class AddCommentToThreadCommand : ICommand<DiscussionThread>
    {
        public Guid ThreadId { get; init; }
        public Comment Comment { get; init; } = default!;
    }
