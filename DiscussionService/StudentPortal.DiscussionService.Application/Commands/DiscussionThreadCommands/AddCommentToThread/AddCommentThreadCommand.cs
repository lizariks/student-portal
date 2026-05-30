using StudentPortal.DiscussionService.Application.DTOs;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;

public class AddCommentToThreadCommand : ICommand<DiscussionThread>
{
    public string ThreadId { get; init; } = default!;
    public CommentRequest Comment { get; init; } = default!;
}
