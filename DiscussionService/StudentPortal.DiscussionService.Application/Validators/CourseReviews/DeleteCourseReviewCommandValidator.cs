using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.DeleteCourseReview;

namespace StudentPortal.DiscussionService.Application.Validators.CourseReviews;

    public class DeleteCourseReviewCommandValidator : AbstractValidator<DeleteCourseReviewCommand>
    {
        public DeleteCourseReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty()
                .WithMessage("ReviewId must be provided.");
        }
    }

