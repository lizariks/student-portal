using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.SearchDiscussionThreads;
    public class SearchDiscussionThreadsQuery : IQuery<IEnumerable<DiscussionThread>>
    {
        public string SearchText { get; init; } = string.Empty;
    }