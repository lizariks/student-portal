using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Infrastructure.Serializers;

public class UserRoleSerializer : SerializerBase<UserRole>
{
    public override UserRole Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var name = context.Reader.ReadString();
        return new UserRole(name);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UserRole value)
    {
        context.Writer.WriteString(value.Name);
    }
}