using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadById;
    public class GetDiscussionThreadByIdQueryHandler : IQueryHandler<GetDiscussionThreadByIdQuery, DiscussionThread?>
    {
        private readonly IDiscussionThreadService _threadService;

        public GetDiscussionThreadByIdQueryHandler(IDiscussionThreadService threadService)
            => _threadService = threadService;

        public async Task<DiscussionThread?> Handle(GetDiscussionThreadByIdQuery request, CancellationToken cancellationToken)
            => await _threadService.GetThreadByIdAsync(request.ThreadId);
    }