namespace StudentPortal.IdentityService.BLL.DTOs;

public class RefreshTokenDto
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}