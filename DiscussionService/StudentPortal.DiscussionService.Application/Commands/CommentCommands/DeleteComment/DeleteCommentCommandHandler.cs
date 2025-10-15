using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using MediatR;

namespace StudentPortal.DiscussionService.Application.Commands.DeleteCommand;

public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly ICommentService _commentService;

    public DeleteCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        await _commentService.DeleteAsync(request.CommentId, cancellationToken);
        return Unit.Value;
    }
}