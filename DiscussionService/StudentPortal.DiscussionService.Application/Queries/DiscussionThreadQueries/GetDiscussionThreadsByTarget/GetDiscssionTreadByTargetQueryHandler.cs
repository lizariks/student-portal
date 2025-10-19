using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadsByTarget;
    public class GetDiscussionThreadsByTargetQueryHandler : IQueryHandler<GetDiscussionThreadsByTargetQuery, IEnumerable<DiscussionThread>>
    {
        private readonly IDiscussionThreadService _threadService;

        public GetDiscussionThreadsByTargetQueryHandler(IDiscussionThreadService threadService)
            => _threadService = threadService;

        public async Task<IEnumerable<DiscussionThread>> Handle(GetDiscussionThreadsByTargetQuery request, CancellationToken cancellationToken)
            => await _threadService.GetThreadsByTargetAsync(request.TargetId, request.TargetType);
    }