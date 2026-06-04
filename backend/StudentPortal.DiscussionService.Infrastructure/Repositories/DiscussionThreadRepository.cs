using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.DiscussionService.Domain.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories
{
    public class DiscussionThreadRepository : MongoRepository<DiscussionThread>, IDiscussionThreadRepository
    {
        public DiscussionThreadRepository(MongoDbContext context, IClientSessionHandle? session = null)
            : base(context, session)
        {
        }

        public async Task<PagedList<DiscussionThread>> GetDiscussionThreadsAsync(
            DiscussionThreadParameters parameters,
            CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<DiscussionThread>.Filter;
            var filter = FilterDefinition<DiscussionThread>.Empty;
            
            if (parameters.TargetType.HasValue)
                filter &= filterBuilder.Eq(t => t.TargetType, parameters.TargetType.Value);

            if (parameters.IsClosed.HasValue)
                filter &= filterBuilder.Eq(t => t.IsClosed, parameters.IsClosed.Value);

            if (parameters.CreatedFrom.HasValue)
                filter &= filterBuilder.Gte(t => t.CreatedAt, parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                filter &= filterBuilder.Lte(t => t.CreatedAt, parameters.CreatedTo.Value);

            var sortHelper = new MongoSortHelper<DiscussionThread>();
            var sort = sortHelper.ApplySort($"{parameters.SortBy} {(parameters.SortDescending ? "desc" : "asc")}");

            var findFluent = _collection.Find(filter).Sort(sort);

            return await PagedList<DiscussionThread>.ToPagedListAsync(
                findFluent,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken
            );
        }

        public async Task<IEnumerable<DiscussionThread>> GetByTargetAsync(string targetId, TargetType targetType, CancellationToken cancellationToken)
        {
            var filter = Builders<DiscussionThread>.Filter.And(
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType)
            );

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DiscussionThread>> SearchByTitleAsync(string searchText, CancellationToken cancellationToken)
        {
            var filter = Builders<DiscussionThread>.Filter.Regex(
                t => t.Title,
                new MongoDB.Bson.BsonRegularExpression(searchText, "i")
            );

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DiscussionThread>> GetClosedThreadsAsync(string targetId, TargetType targetType, CancellationToken cancellationToken)
        {
            var filter = Builders<DiscussionThread>.Filter.And(
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType),
                Builders<DiscussionThread>.Filter.Eq(t => t.IsClosed, true)
            );

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<long> GetThreadCountByTargetAsync(string targetId, TargetType targetType, CancellationToken cancellationToken)
        {
            var filter = Builders<DiscussionThread>.Filter.And(
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetId, targetId),
                Builders<DiscussionThread>.Filter.Eq(t => t.TargetType, targetType)
            );

            return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        }
    }
}
