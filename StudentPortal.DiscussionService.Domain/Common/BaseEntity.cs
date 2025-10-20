namespace StudentPortal.DiscussionService.Domain.Common;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; private set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; private set; }

        protected BaseEntity()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
