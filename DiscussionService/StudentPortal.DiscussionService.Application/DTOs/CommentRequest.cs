namespace StudentPortal.DiscussionService.Application.DTOs;

public sealed class CommentRequest
{
    public UserInfoRequest Author { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string? ParentCommentId { get; init; }
}

public sealed class UserInfoRequest
{
    public string UserId { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public UserRoleRequest Role { get; init; } = default!;
}

public sealed class UserRoleRequest
{
    public string Name { get; init; } = default!;
}
