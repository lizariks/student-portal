namespace StudentPortal.AggregatorService.DTOs.Discussion;

public class CommentDto
{
    public string Id { get; set; } = null!;
    public string? ParentCommentId { get; set; }
    public PublicUserDto Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
}