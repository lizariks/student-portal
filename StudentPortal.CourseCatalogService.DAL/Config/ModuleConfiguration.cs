namespace StudentPortal.CourseCatalogService.DAL.Config;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> b)
    {
        b.ToTable("Modules");
        b.HasKey(m => m.Id);
        b.Property(m => m.Title).IsRequired().HasMaxLength(200);
        b.Property(m => m.Description).HasMaxLength(2000);
        b.Property(m => m.Order).IsRequired();
        b.HasIndex(m => new { m.CourseId, m.Order }).IsUnique();
    }
}