using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;

namespace StudentPortal.DiscussionService.Application.Validators.DiscussionThreads;
    public class AddCommentToThreadCommandValidator : AbstractValidator<AddCommentToThreadCommand>
    {
        public AddCommentToThreadCommandValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId must be provided.");

            RuleFor(x => x.Comment)
                .NotNull()
                .WithMessage("Comment must be provided.");

            RuleFor(x => x.Comment.Content)
                .NotEmpty()
                .MaximumLength(500)
                .WithMessage("Comment content must be provided and cannot exceed 500 characters.");

            RuleFor(x => x.Comment.Author)
                .NotNull()
                .WithMessage("Comment author must be provided.");
        }
    }