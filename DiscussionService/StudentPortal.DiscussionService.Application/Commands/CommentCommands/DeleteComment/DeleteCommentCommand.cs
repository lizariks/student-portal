using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;

namespace StudentPortal.DiscussionService.Application.Commands.CommentCommands.DeleteComment;
public class DeleteCommentCommand : ICommand
{
    public Guid CommentId { get; init; } = default!;

   
}