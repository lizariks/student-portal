using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;
namespace StudentPortal.DiscussionService.Application.Commands.UpdateCommand;

public class UpdateCommentCommand : ICommand<Comment>
{
    public Guid CommentId { get; init; }
    public string NewContent { get; init; } = default!;
    public UserInfo Actor { get; init; } = default!;

    public UpdateCommentCommand(Guid commentId, string newContent, UserInfo actor)
    {
        CommentId = commentId;
        NewContent = newContent;
        Actor = actor;
    }
}