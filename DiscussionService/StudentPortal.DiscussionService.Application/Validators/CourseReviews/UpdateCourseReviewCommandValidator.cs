using FluentValidation;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.UpdateCourseReview;

namespace StudentPortal.DiscussionService.Application.Validators.CourseReviews;
    public class UpdateCourseReviewCommandValidator : AbstractValidator<UpdateCourseReviewCommand>
    {
        public UpdateCourseReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty()
                .WithMessage("ReviewId must be provided.");

            RuleFor(x => x.NewRatingValue)
                .InclusiveBetween(1, 5) 
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.NewComment)
                .NotEmpty()
                .MaximumLength(1000)
                .WithMessage("Comment must be provided and cannot exceed 1000 characters.");
        }
    }