namespace StudentPortal.IdentityService.BLL.DTOs;
using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = null!;
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    [Required, MinLength(8)]
    public string Password { get; set; } = null!;
}