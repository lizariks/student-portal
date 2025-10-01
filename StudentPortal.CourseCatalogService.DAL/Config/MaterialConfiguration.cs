namespace StudentPortal.CourseCatalogService.DAL.Config;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> b)
    {
        b.ToTable("Materials");
        b.HasKey(m => m.Id);
        b.Property(m => m.Title).IsRequired().HasMaxLength(300);
        b.Property(m => m.Url).HasMaxLength(2000);
        b.Property(m => m.Type).IsRequired();
        b.Property(m => m.Order).IsRequired();
        b.HasIndex(m => new { m.LessonId, m.Order }).IsUnique();
    }
}