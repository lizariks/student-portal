using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.DeleteCourseReview;

    public class DeleteCourseReviewCommandHandler : ICommandHandler<DeleteCourseReviewCommand, bool>
    {
        private readonly ICourseReviewService _courseReviewService;

        public DeleteCourseReviewCommandHandler(ICourseReviewService courseReviewService)
        {
            _courseReviewService = courseReviewService;
        }

        public async Task<bool> Handle(DeleteCourseReviewCommand request, CancellationToken cancellationToken)
        {
            await _courseReviewService.DeleteReviewAsync(request.ReviewId);
            return true;
        }
    }