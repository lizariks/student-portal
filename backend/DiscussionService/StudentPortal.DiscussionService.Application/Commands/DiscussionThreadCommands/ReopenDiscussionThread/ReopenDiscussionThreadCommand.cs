using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ReopenDiscussionThread;

    public class ReopenDiscussionThreadCommand : ICommand<DiscussionThread>
    {
        public string ThreadId { get; init; }
        public UserInfo Actor { get; init; } = default!;
    }

