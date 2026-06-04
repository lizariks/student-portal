using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.DeleteComment;

namespace StudentPortal.DiscussionService.Application.Commands.CommentCommands.DeleteComment;

    public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID must be provided.");
        }
    }
