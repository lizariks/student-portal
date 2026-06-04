
using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetCommentById;
    public class GetCommentByIdQuery : IQuery<Comment?>
    {
        public string CommentId { get; init; }
    }
