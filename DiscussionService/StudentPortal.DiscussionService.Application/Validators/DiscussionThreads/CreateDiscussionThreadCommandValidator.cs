using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;

namespace StudentPortal.DiscussionService.Application.Validators.DiscussionThreads;
    public class CreateDiscussionThreadCommandValidator : AbstractValidator<CreateDiscussionThreadCommand>
    {
        public CreateDiscussionThreadCommandValidator()
        {
            RuleFor(x => x.TargetId)
                .NotEmpty()
                .WithMessage("TargetId must be provided.");

            RuleFor(x => x.CreatedBy)
                .NotNull()
                .WithMessage("CreatedBy information must be provided.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150)
                .WithMessage("Title must be provided and cannot exceed 150 characters.");
        }
    }
