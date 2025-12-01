namespace StudentPortal.IdentityService.BLL.DTOs;


using System.ComponentModel.DataAnnotations;

public class VerifyEmailDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}