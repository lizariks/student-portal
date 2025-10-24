using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;

public class CloseDiscussionThreadCommand : ICommand
{
    public string ThreadId { get; }
    public UserInfo Actor { get; }

    public CloseDiscussionThreadCommand(string threadId, UserInfo actor)
    {
        ThreadId = threadId;
        Actor = actor;
    }
}
