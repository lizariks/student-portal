namespace StudentPortal.DiscussionService.Domain.Entities;

using StudentPortal.DiscussionService.Domain.Exceptions;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Domain.Entities.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class CourseReview : BaseEntity
{
    [BsonElement("targetId")]
    public Guid TargetId { get; private set; }

    [BsonElement("targetType")]
    public TargetType TargetType { get; private set; }

    [BsonElement("reviewer")]
    public UserInfo Reviewer { get; private set; }

    [BsonElement("rating")]
    public RatingValue Rating { get; private set; }

    [BsonElement("comment")]
    public string Comment { get; private set; }

    private CourseReview() { }

    public CourseReview(Guid targetId, TargetType targetType, UserInfo reviewer, RatingValue rating, string comment)
    {
        if (targetId == Guid.Empty)
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));

        if (string.IsNullOrWhiteSpace(comment) || comment.Length > 1000)
            throw new InvalidContentException();

        TargetId = targetId;
        TargetType = targetType;
        Reviewer = reviewer ?? throw new ArgumentNullException(nameof(reviewer));
        Rating = rating ?? throw new ArgumentNullException(nameof(rating));
        Comment = comment.Trim();
    }

    public void UpdateRating(int newValue)
    {
        Rating = new RatingValue(newValue);
        MarkUpdated();
    }

    public void UpdateComment(string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment) || newComment.Length > 1000)
            throw new InvalidContentException();

        Comment = newComment.Trim();
        MarkUpdated();
    }
}