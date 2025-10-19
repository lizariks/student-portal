using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetComments;
    public class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, IEnumerable<Comment>>
    {
        private readonly ICommentService _commentService;

        public GetCommentsQueryHandler(ICommentService commentService)
            => _commentService = commentService;

        public async Task<IEnumerable<Comment>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
            => await _commentService.SearchByContentAsync(string.Empty, cancellationToken);
    }