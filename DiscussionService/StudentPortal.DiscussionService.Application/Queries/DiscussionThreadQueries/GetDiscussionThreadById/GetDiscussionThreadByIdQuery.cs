using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadById;
    public class GetDiscussionThreadByIdQuery : IQuery<DiscussionThread?>
    {
        public string ThreadId { get; init; }
    }