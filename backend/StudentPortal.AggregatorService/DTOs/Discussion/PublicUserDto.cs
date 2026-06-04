namespace StudentPortal.AggregatorService.DTOs.Discussion;

public class PublicUserDto
{
    public string UserId { get; set; } = null!; 
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}