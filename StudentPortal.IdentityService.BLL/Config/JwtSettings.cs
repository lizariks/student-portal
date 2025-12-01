namespace StudentPortal.IdentityService.BLL.Config;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenDurationMinutes { get; set; } = 15;
    public int RefreshTokenDurationDays { get; set; } = 7;
}