using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Common;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;

namespace StudentPortal.DiscussionService.Infrastructure.Repositories
{
    public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly IClientSessionHandle? _session;

        public MongoRepository(MongoDbContext context, IClientSessionHandle? session = null)
        {
            _collection = typeof(T).Name switch
            {
                "Comment" => (IMongoCollection<T>)context.Comments,
                "CourseReview" => (IMongoCollection<T>)context.CourseReviews,
                "DiscussionThread" => (IMongoCollection<T>)context.DiscussionThreads,
                _ => context.Database.GetCollection<T>(typeof(T).Name + "s")
            };

            _session = session;
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (_session != null)
                await _collection.InsertOneAsync(_session, entity, cancellationToken: cancellationToken);
            else
                await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

            return entity;
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Eq("_id", id); 
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }


        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Eq("_id", id); 
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }

        
    }
}
