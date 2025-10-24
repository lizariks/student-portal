using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;
    public class ResolveDiscussionThreadCommand : ICommand<DiscussionThread>
    {
        public string ThreadId { get; init; }
        public string CommentId { get; init; }
        public UserInfo Actor { get; init; } = default!;
    }
