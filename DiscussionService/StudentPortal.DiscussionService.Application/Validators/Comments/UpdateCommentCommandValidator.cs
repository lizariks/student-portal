using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.UpdateComment;

namespace StudentPortal.DiscussionService.Application.Validators.Comments;
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID must be provided.");

            RuleFor(x => x.NewContent)
                .NotEmpty()
                .WithMessage("Updated content cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Updated content cannot exceed 1000 characters.");

            RuleFor(x => x.Actor)
                .NotNull()
                .WithMessage("Actor information must be provided.");

            RuleFor(x => x.Actor.UserId)
                .NotEmpty()
                .WithMessage("Actor UserId cannot be empty.");
        }
    }