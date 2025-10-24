using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;
namespace StudentPortal.DiscussionService.Application.Commands.CommentCommands.UpdateComment;

public class UpdateCommentCommand : ICommand<Comment>
{
    public string CommentId { get; init; }
    public string NewContent { get; init; } = default!;
    public UserInfo Actor { get; init; } = default!;

    public UpdateCommentCommand(string commentId, string newContent, UserInfo actor)
    {
        if (string.IsNullOrWhiteSpace(commentId))
            throw new ArgumentException("CommentId cannot be empty.", nameof(commentId));

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("NewContent cannot be empty.", nameof(newContent));

        if (actor == null)
            throw new ArgumentNullException(nameof(actor));

        CommentId = commentId;
        NewContent = newContent;
        Actor = actor;
    }

}