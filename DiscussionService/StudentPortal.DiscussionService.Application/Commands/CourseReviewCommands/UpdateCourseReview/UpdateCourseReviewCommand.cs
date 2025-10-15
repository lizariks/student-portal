using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.UpdateCourseReview;

    public class UpdateCourseReviewCommand : ICommand<CourseReview>
    {
        public Guid ReviewId { get; init; }
        public int NewRatingValue { get; init; }
        public string NewComment { get; init; } = default!;
    }

