namespace StudentPortal.CourseCatalogService.DAL.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Email).IsRequired().HasMaxLength(200);
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.Nickname)
            .HasMaxLength(50);
        b.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);
        b.Property(u => u.Role).IsRequired();
    }
}