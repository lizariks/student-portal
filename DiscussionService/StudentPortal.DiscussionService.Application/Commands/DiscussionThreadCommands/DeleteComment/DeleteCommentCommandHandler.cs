using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.DeleteComment;

public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, DiscussionThread>
{
    private readonly IDiscussionThreadService _discussionThreadService;

    public DeleteCommentCommandHandler(IDiscussionThreadService discussionThreadService)
    {
        _discussionThreadService = discussionThreadService;
    }

    public async Task<DiscussionThread> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var actor = new UserInfo(
            request.Actor.UserId,
            request.Actor.UserName,
            new UserRole(request.Actor.Role.Name));

        await _discussionThreadService.DeleteCommentAsync(request.ThreadId, request.CommentId, actor, cancellationToken);

        var updatedThread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId, cancellationToken);
        return updatedThread!;
    }
}
