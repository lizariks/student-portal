using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;
    public class CloseDiscussionThreadCommandValidator : AbstractValidator<CloseDiscussionThreadCommand>
    {
        public CloseDiscussionThreadCommandValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId must be provided.");

            RuleFor(x => x.Actor)
                .NotNull()
                .WithMessage("Actor must be provided.");
        }
    }
