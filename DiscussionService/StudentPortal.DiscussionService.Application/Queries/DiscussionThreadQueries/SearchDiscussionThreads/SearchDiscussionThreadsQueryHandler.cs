using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.SearchDiscussionThreads;
    public class SearchDiscussionThreadsQueryHandler : IQueryHandler<SearchDiscussionThreadsQuery, IEnumerable<DiscussionThread>>
    {
        private readonly IDiscussionThreadService _threadService;

        public SearchDiscussionThreadsQueryHandler(IDiscussionThreadService threadService)
            => _threadService = threadService;

        public async Task<IEnumerable<DiscussionThread>> Handle(SearchDiscussionThreadsQuery request, CancellationToken cancellationToken)
            => await _threadService.SearchThreadsAsync(request.SearchText);
    }