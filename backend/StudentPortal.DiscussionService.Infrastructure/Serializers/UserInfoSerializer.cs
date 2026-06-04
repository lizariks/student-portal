using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using System;

namespace StudentPortal.DiscussionService.Infrastructure.Serializers
{
    public class UserInfoSerializer : SerializerBase<UserInfo>
    {
        public override UserInfo Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            reader.ReadStartDocument();

            var userId = reader.ReadString(); 
            var userName = reader.ReadString();
            var roleName = reader.ReadString();

            reader.ReadEndDocument();
            var role = new UserRole(roleName);
            return new UserInfo(userId, userName, role);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UserInfo value)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();
            writer.WriteString(value.UserId);
            writer.WriteString(value.UserName);
            writer.WriteString(value.Role.Name);

            writer.WriteEndDocument();
        }
    }
}