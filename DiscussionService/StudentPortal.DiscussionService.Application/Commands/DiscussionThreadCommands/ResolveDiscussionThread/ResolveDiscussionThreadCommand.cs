using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;
    public class ResolveDiscussionThreadCommand : ICommand<DiscussionThread>
    {
        public Guid ThreadId { get; init; }
        public Guid CommentId { get; init; }
        public UserInfo Actor { get; init; } = default!;
    }
