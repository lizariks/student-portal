using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;

public class CloseDiscussionThreadCommand : ICommand
{
    public Guid ThreadId { get; }
    public UserInfo Actor { get; }

    public CloseDiscussionThreadCommand(Guid threadId, UserInfo actor)
    {
        ThreadId = threadId;
        Actor = actor;
    }
}
