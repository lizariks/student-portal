namespace StudentPortal.CourseCatalogService.DAL.Config;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.Name).IsUnique();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
