namespace StudentPortal.IdentityService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}