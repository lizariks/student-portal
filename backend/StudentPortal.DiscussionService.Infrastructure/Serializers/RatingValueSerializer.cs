using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Infrastructure.Serializers;

public class RatingValueSerializer : SerializerBase<RatingValue>
{
    public override RatingValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var value = context.Reader.ReadInt32();
        return new RatingValue(value);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, RatingValue value)
    {
        context.Writer.WriteInt32(value.Value);
    }
}