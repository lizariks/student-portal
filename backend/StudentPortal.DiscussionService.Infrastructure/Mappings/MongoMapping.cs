using MongoDB.Bson.Serialization;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Infrastructure.Serializers;

namespace StudentPortal.DiscussionService.Infrastructure.Mappings
{
    public static class MongoMappings
    {
        public static void Register()
        {
            BsonSerializer.RegisterSerializer(new RatingValueSerializer());
            BsonSerializer.RegisterSerializer(new UserRoleSerializer());
            BsonSerializer.RegisterSerializer(new UserInfoSerializer());

            if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
            {
                BsonClassMap.RegisterClassMap<Comment>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.MapProperty(c => c.Author);
                    cm.MapProperty(c => c.Content);
                    cm.MapProperty(c => c.ParentCommentId);
                    cm.MapProperty(c => c.CreatedAt);
                    cm.MapProperty(c => c.IsResolved);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(CourseReview)))
            {
                BsonClassMap.RegisterClassMap<CourseReview>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.MapProperty(c => c.TargetId);
                    cm.MapProperty(c => c.TargetType);
                    cm.MapProperty(c => c.Reviewer);
                    cm.MapProperty(c => c.Rating);
                    cm.MapProperty(c => c.Comment);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(DiscussionThread)))
            {
                BsonClassMap.RegisterClassMap<DiscussionThread>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.MapProperty(c => c.TargetId);
                    cm.MapProperty(c => c.TargetType);
                    cm.MapProperty(c => c.Title);
                    cm.MapProperty(c => c.CreatedBy);
                    cm.MapProperty(c => c.IsClosed);
                    cm.MapField("_comments"); 
                });
            }
        }
    }
}
