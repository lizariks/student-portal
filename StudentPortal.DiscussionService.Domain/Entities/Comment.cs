using StudentPortal.DiscussionService.Domain.Common;

namespace StudentPortal.DiscussionService.Domain.Entities;

using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using StudentPortal.DiscussionService.Domain.Common;


public class Comment : BaseEntity
{
   

    [BsonElement("parentCommentId")]
    [BsonRepresentation(BsonType.String)]
    public Guid? ParentCommentId { get; private set; }

    [BsonElement("author")]
    public UserInfo Author { get; private set; }

    [BsonElement("content")]
    public string Content { get; private set; }

    [BsonElement("isResolved")]
    public bool IsResolved { get; private set; }

    private Comment() { }

    public Comment(UserInfo author, string content, Guid? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            throw new InvalidContentException();

        Author = author ?? throw new ArgumentNullException(nameof(author));
        Content = content.Trim();
        ParentCommentId = parentCommentId;
        IsResolved = false;
    }

    public void Edit(string newContent, UserInfo actor)
    {
        if (actor.UserId != Author.UserId && actor.Role.Name != "Admin")
            throw new UnauthorizedActionException();

        if (string.IsNullOrWhiteSpace(newContent) || newContent.Length > 500)
            throw new InvalidContentException();

        Content = newContent.Trim();
    }

    public void MarkAsResolved(UserInfo actor)
    {
        if (actor.Role.Name == "Student")
            throw new UnauthorizedActionException();

        IsResolved = true;
    }
}
