namespace StudentPortal.DiscussionService.Domain.Entities;

using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using StudentPortal.DiscussionService.Domain.Common;



public class DiscussionThread : BaseEntity
{
    [BsonElement("targetId")]
    [BsonRepresentation(BsonType.String)]
    public Guid TargetId { get; private set; }

    [BsonElement("targetType")]
    public TargetType TargetType { get; private set; }

    [BsonElement("title")]
    public string Title { get; private set; }

    [BsonElement("createdBy")]
    public UserInfo CreatedBy { get; private set; }

    [BsonElement("isClosed")]
    public bool IsClosed { get; private set; }

    [BsonElement("comments")]
    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private DiscussionThread() { }

    public DiscussionThread(Guid targetId, TargetType targetType, string title, UserInfo createdBy)
    {
        if (targetId == Guid.Empty)
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));

        if (string.IsNullOrWhiteSpace(title) || title.Length > 150)
            throw new InvalidContentException();

        TargetId = targetId;
        TargetType = targetType;
        Title = title.Trim();
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        IsClosed = false;
    }

    public void AddComment(Comment comment)
    {
        if (IsClosed)
            throw new ThreadClosedException();

        _comments.Add(comment ?? throw new ArgumentNullException(nameof(comment)));
        MarkUpdated();
    }

    public void Close(UserInfo actor)
    {
        if (actor.Role == UserRole.Student)
            throw new UnauthorizedActionException();

        IsClosed = true;
        MarkUpdated();
    }

    public void Reopen(UserInfo actor)
    {
        if (actor.Role == UserRole.Student)
            throw new UnauthorizedActionException();

        IsClosed = false;
        MarkUpdated();
    }
}
