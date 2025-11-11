using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;
    public class EditDiscussionThreadCommentCommandValidator : AbstractValidator<EditDiscussionThreadCommentCommand>
    {
        public EditDiscussionThreadCommentCommandValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId must be provided.");

            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId must be provided.");

            RuleFor(x => x.NewContent)
                .NotEmpty()
                .MaximumLength(500)
                .WithMessage("New content must be provided and cannot exceed 500 characters.");

            RuleFor(x => x.Actor)
                .NotNull()
                .WithMessage("Actor must be provided.");
        }
    }