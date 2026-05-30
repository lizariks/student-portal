using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;

public class AddCommentToThreadCommandHandler : ICommandHandler<AddCommentToThreadCommand, DiscussionThread>
{
    private readonly IDiscussionThreadService _discussionThreadService;

    public AddCommentToThreadCommandHandler(IDiscussionThreadService discussionThreadService)
    {
        _discussionThreadService = discussionThreadService;
    }

    public async Task<DiscussionThread> Handle(AddCommentToThreadCommand request, CancellationToken cancellationToken)
    {
        if (request.Comment == null)
            throw new ArgumentNullException(nameof(request.Comment), "Comment cannot be null.");

        var thread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
        if (thread == null)
            throw new InvalidOperationException($"Discussion thread with ID '{request.ThreadId}' not found.");

        var author = new UserInfo(
            request.Comment.Author.UserId,
            request.Comment.Author.UserName,
            new UserRole(request.Comment.Author.Role.Name));

        var comment = new Comment(author, request.Comment.Content, request.Comment.ParentCommentId);

        await _discussionThreadService.AddCommentAsync(request.ThreadId, comment);

        var updatedThread = await _discussionThreadService.GetThreadByIdAsync(request.ThreadId);
        return updatedThread!;
    }
}
