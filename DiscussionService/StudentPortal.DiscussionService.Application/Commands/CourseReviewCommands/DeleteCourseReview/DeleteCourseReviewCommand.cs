using StudentPortal.DiscussionService.Application.Interfaces.Commands;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.DeleteCourseReview;

    public class DeleteCourseReviewCommand : ICommand<bool>
    {
        public Guid ReviewId { get; init; }
    }