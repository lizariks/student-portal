namespace StudentPortal.IdentityService.Domain.Entities;

using Microsoft.AspNetCore.Identity;


    public class 
        ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
