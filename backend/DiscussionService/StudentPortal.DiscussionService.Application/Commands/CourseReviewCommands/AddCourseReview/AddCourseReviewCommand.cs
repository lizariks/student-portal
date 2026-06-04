
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;

    public class AddCourseReviewCommand : ICommand<CourseReview>
    {
        public string TargetId { get; init; }
        public TargetType TargetType { get; init; }
        public UserInfo Reviewer { get; init; } = default!;
        public RatingValue Rating { get; init; } = default!;
        public string Comment { get; init; } = default!;
    }

