using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DeleteCommand;

namespace StudentPortal.DiscussionService.Application.Validators.Comments;

    public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID must be provided.");
        }
    }
