namespace StudentPortal.IdentityService.BLL.DTOs;

using Microsoft.AspNetCore.Identity;

public class RegisterResultDto
{
    public bool Succeeded { get; set; }
    public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();
    
    public string? EmailConfirmationToken { get; set; } 
}