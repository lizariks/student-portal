using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.UpdateCourseReview;

    public class UpdateCourseReviewCommandHandler : ICommandHandler<UpdateCourseReviewCommand, CourseReview>
    {
        private readonly ICourseReviewService _courseReviewService;

        public UpdateCourseReviewCommandHandler(ICourseReviewService courseReviewService)
        {
            _courseReviewService = courseReviewService;
        }

        public async Task<CourseReview> Handle(UpdateCourseReviewCommand request, CancellationToken cancellationToken)
        {
            return await _courseReviewService.UpdateReviewAsync(
                request.ReviewId,
                request.NewRatingValue,
                request.NewComment
            );
        }
    }