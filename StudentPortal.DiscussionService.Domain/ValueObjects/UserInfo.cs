namespace StudentPortal.DiscussionService.Domain.ValueObjects;


    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson;
    using System;

    public class UserInfo : ValueObject
    {
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; private set; }

        [BsonElement("userName")]
        public string UserName { get; private set; }

        [BsonElement("role")]
        public UserRole Role { get; private set; }

        private UserInfo() { }

        public UserInfo(Guid userId, string userName, UserRole role)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));

            Role = role ?? throw new ArgumentNullException(nameof(role));

            UserId = userId;
            UserName = userName.Trim();
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return UserId;
            yield return UserName.ToLowerInvariant();
            yield return Role;
        }

        public override string ToString() => $"{UserName} ({Role.Name})";
    }

    public class UserRole : ValueObject
    {
        [BsonElement("name")]
        public string Name { get; private set; }

        private UserRole() { }

        public UserRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty.", nameof(name));

            Name = name.Trim();
        }

        public static UserRole Student => new("Student");
        public static UserRole Instructor => new("Instructor");
        public static UserRole Admin => new("Admin");

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name.ToLowerInvariant();
        }

        public override string ToString() => Name;
    }