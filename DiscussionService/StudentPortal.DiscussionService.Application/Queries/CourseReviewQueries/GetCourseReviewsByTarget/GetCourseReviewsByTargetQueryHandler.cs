using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewsByTarget;
    public class GetCourseReviewsByTargetQueryHandler : IQueryHandler<GetCourseReviewsByTargetQuery, IEnumerable<CourseReview>>
    {
        private readonly ICourseReviewService _courseReviewService;

        public GetCourseReviewsByTargetQueryHandler(ICourseReviewService courseReviewService)
            => _courseReviewService = courseReviewService;

        public async Task<IEnumerable<CourseReview>> Handle(GetCourseReviewsByTargetQuery request, CancellationToken cancellationToken)
            => await _courseReviewService.GetReviewsByTargetAsync(request.TargetId, request.TargetType);
    }