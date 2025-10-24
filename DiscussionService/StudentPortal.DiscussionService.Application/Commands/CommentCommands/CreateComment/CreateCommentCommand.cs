using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Commands.CommentCommands.CreateComment;
public class CreateCommentCommand : ICommand<Comment>
{
    public UserInfo Author { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string? ParentCommentId { get; init; }
}

