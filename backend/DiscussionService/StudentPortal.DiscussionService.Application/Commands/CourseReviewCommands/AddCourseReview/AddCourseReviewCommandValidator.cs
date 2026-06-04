using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;
    public class AddCourseReviewCommandValidator : AbstractValidator<AddCourseReviewCommand>
    {
        public AddCourseReviewCommandValidator()
        {
            RuleFor(x => x.TargetId)
                .NotEmpty()
                .WithMessage("TargetId must be provided.");

            RuleFor(x => x.Reviewer)
                .NotNull()
                .WithMessage("Reviewer information must be provided.");

            RuleFor(x => x.Rating)
                .NotNull()
                .WithMessage("Rating must be provided.");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .MaximumLength(1000)
                .WithMessage("Comment must be provided and cannot exceed 1000 characters.");
        }
    }
