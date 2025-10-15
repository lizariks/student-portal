using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.UpdateCommand;

public class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand, Comment>
{
    private readonly ICommentService _commentService;

    public UpdateCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<Comment> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentService.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            throw new NotFoundException($"Comment with ID '{request.CommentId}' was not found.");

        comment.Edit(request.NewContent, request.Actor);

        await _commentService.UpdateAsync(comment, cancellationToken);

        return comment;
    }
}