namespace StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

    public class RatingValue : ValueObject
    {
        [BsonElement("value")]
        public int Value { get; private set; }

        private RatingValue() { }

        public RatingValue(int value)
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }
