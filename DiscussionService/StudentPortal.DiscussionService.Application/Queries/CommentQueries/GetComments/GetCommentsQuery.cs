using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetComments;
    public class GetCommentsQuery : IQuery<IEnumerable<Comment>> { }
