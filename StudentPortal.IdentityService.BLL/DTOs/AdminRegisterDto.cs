namespace StudentPortal.IdentityService.BLL.DTOs;

using System.ComponentModel.DataAnnotations;

public class AdminRegisterDto
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = "Student";
}