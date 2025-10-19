using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewById;
    public class GetCourseReviewByIdQuery : IQuery<CourseReview?>
    {
        public Guid ReviewId { get; init; }
    }