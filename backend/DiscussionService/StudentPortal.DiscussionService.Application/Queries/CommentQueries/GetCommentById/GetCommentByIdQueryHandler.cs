
using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

namespace StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetCommentById;
    public class GetCommentByIdQueryHandler : IQueryHandler<GetCommentByIdQuery, Comment?>
    {
        private readonly ICommentService _commentService;

        public GetCommentByIdQueryHandler(ICommentService commentService)
            => _commentService = commentService;

        public async Task<Comment?> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
            => await _commentService.GetByIdAsync(request.CommentId, cancellationToken);
    }
