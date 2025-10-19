using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;

namespace StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewsByTarget;
    public class GetCourseReviewsByTargetQuery : IQuery<IEnumerable<CourseReview>>
    {
        public Guid TargetId { get; init; }
        public TargetType TargetType { get; init; }
    }