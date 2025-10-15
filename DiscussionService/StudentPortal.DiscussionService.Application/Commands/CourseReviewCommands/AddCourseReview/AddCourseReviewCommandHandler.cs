using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;

    public class AddCourseReviewCommandHandler : ICommandHandler<AddCourseReviewCommand, CourseReview>
    {
        private readonly ICourseReviewService _courseReviewService;

        public AddCourseReviewCommandHandler(ICourseReviewService courseReviewService)
        {
            _courseReviewService = courseReviewService;
        }

        public async Task<CourseReview> Handle(AddCourseReviewCommand request, CancellationToken cancellationToken)
        {
            var review = new CourseReview(
                request.TargetId,
                request.TargetType,
                request.Reviewer,
                request.Rating,
                request.Comment
            );

            await _courseReviewService.AddReviewAsync(
                request.TargetId,
                request.TargetType,
                request.Reviewer,
                request.Rating.Value,
                request.Comment
            );

            return review;
        }
    }