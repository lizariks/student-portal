using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;

namespace StudentPortal.DiscussionService.Application.Validators.DiscussionThreads;
    public class ResolveDiscussionThreadCommandValidator : AbstractValidator<ResolveDiscussionThreadCommand>
    {
        public ResolveDiscussionThreadCommandValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId must be provided.");

            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId must be provided.");

            RuleFor(x => x.Actor)
                .NotNull()
                .WithMessage("Actor must be provided.");
        }
    }