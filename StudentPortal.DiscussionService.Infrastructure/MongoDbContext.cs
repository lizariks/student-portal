
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
namespace StudentPortal.DiscussionService.Infrastructure;

 public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public IMongoDatabase Database => _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var mongoSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
            mongoSettings.MaxConnectionPoolSize = settings.Value.MaxConnectionPoolSize;
            mongoSettings.MinConnectionPoolSize = settings.Value.MinConnectionPoolSize;
            mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(settings.Value.ConnectTimeoutSeconds);
            mongoSettings.SocketTimeout = TimeSpan.FromSeconds(settings.Value.SocketTimeoutSeconds);

            var client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
        public IMongoCollection<CourseReview> CourseReviews => _database.GetCollection<CourseReview>("course-reviews");
        public IMongoCollection<DiscussionThread> DiscussionThreads => _database.GetCollection<DiscussionThread>("discussion-threads");

        public IClientSessionHandle StartSession() => _database.Client.StartSession();
    }

