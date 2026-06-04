using StudentPortal.DiscussionService.Application.DTOs;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;

public class EditDiscussionThreadCommentCommand : ICommand<DiscussionThread>
{
    public string ThreadId { get; init; } = default!;
    public string CommentId { get; init; } = default!;
    public string NewContent { get; init; } = default!;
    public UserInfoRequest Actor { get; init; } = default!;
}