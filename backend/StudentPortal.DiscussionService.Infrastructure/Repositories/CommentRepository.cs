using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.DiscussionService.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories
{
    public class CommentRepository : MongoRepository<Comment>, ICommentRepository
    {
        private readonly IDiscussionThreadRepository _threadRepository;

        public CommentRepository(
            MongoDbContext context,
            IDiscussionThreadRepository threadRepository,
            IClientSessionHandle? session = null)
            : base(context, session)
        {
            _threadRepository = threadRepository;
        }

        public async Task<PagedList<Comment>> GetCommentsAsync(CommentParameters parameters, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Comment>.Filter;
            var filter = FilterDefinition<Comment>.Empty;
            
            if (parameters.IsResolved.HasValue)
                filter &= filterBuilder.Eq(c => c.IsResolved, parameters.IsResolved.Value);

            if (parameters.CreatedFrom.HasValue)
                filter &= filterBuilder.Gte(c => c.CreatedAt, parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                filter &= filterBuilder.Lte(c => c.CreatedAt, parameters.CreatedTo.Value);

            var sortHelper = new MongoSortHelper<Comment>();

            var findFluent = _collection
                .Find(filter)
                .Sort(sortHelper.ApplySort(parameters.OrderBy ?? parameters.SortBy));

            return await PagedList<Comment>.ToPagedListAsync(
                findFluent,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken
            );
        }

        public async Task<IEnumerable<Comment>> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                return Enumerable.Empty<Comment>();

            return thread.Comments ?? Enumerable.Empty<Comment>();
        }

        public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId, CancellationToken cancellationToken)
        {
            var filter = Builders<Comment>.Filter.Eq("author.userId", authorId);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comment>> SearchByContentAsync(string keyword, CancellationToken cancellationToken)
        {
            var filter = Builders<Comment>.Filter.Regex(
                c => c.Content,
                new MongoDB.Bson.BsonRegularExpression(keyword, "i")
            );

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<long> GetThreadCommentCountAsync(string threadId, CancellationToken cancellationToken)
        {
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null)
                return 0;

            return thread.Comments?.Count() ?? 0;
        }
    }
}
