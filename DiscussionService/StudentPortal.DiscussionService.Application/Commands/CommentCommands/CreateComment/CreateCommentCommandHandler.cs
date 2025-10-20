namespace StudentPortal.DiscussionService.Application.Commands.CommentCommands.CreateComment;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;


public class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, Comment>
{
    private readonly ICommentService _commentService;

    public CreateCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<Comment> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment(request.Author, request.Content, request.ParentCommentId);
        await _commentService.AddCommentAsync(comment, cancellationToken);
        return comment;
    }
}
