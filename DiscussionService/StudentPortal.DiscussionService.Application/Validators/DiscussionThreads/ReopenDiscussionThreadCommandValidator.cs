using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ReopenDiscussionThread;

namespace StudentPortal.DiscussionService.Application.Validators.DiscussionThreads;
    public class ReopenDiscussionThreadCommandValidator : AbstractValidator<ReopenDiscussionThreadCommand>
    {
        public ReopenDiscussionThreadCommandValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId must be provided.");

            RuleFor(x => x.Actor)
                .NotNull()
                .WithMessage("Actor must be provided.");
        }
    }
