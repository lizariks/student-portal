namespace StudentPortal.CourseCatalogService.DAL.Config;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.ToTable("Lessons");
        b.HasKey(l => l.Id);
        b.Property(l => l.Title).IsRequired().HasMaxLength(200);
        b.Property(l => l.Content).HasColumnType("nvarchar(max)");
        b.Property(l => l.Order).IsRequired();
        b.HasIndex(l => new { l.ModuleId, l.Order }).IsUnique();
    }
}