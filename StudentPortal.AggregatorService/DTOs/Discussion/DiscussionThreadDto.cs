namespace StudentPortal.AggregatorService.DTOs.Discussion;
using StudentPortal.DiscussionService.Domain.Enums;

public class DiscussionThreadDto
{
    public string Id { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public TargetType TargetType { get; set; }
    public string Title { get; set; } = null!;

    public PublicUserDto CreatedBy { get; set; } = null!; 

    public bool IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
}