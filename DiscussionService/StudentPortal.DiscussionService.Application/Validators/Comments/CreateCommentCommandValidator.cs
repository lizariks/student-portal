using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.CreateComment;

namespace StudentPortal.DiscussionService.Application.Validators.Comments;

    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.Author)
                .NotNull()
                .WithMessage("Author information must be provided.");

            RuleFor(x => x.Author.UserId)
                .NotEmpty()
                .WithMessage("Author UserId cannot be empty.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Comment content cannot exceed 1000 characters.");

            RuleFor(x => x.ParentCommentId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("If provided, ParentCommentId must be a valid GUID.");
        }
    }
