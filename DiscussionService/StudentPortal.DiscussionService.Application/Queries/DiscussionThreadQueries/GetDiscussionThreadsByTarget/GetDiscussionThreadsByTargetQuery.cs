using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadsByTarget;
    public class GetDiscussionThreadsByTargetQuery : IQuery<IEnumerable<DiscussionThread>>
    {
        public Guid TargetId { get; init; }
        public TargetType TargetType { get; init; }
    }
