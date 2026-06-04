using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewById;
    public class GetCourseReviewByIdQueryHandler : IQueryHandler<GetCourseReviewByIdQuery, CourseReview?>
    {
        private readonly ICourseReviewService _courseReviewService;

        public GetCourseReviewByIdQueryHandler(ICourseReviewService courseReviewService)
            => _courseReviewService = courseReviewService;

        public async Task<CourseReview?> Handle(GetCourseReviewByIdQuery request, CancellationToken cancellationToken)
            => await _courseReviewService.GetReviewByIdAsync(request.ReviewId);
    }
