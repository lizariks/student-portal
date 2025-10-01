namespace StudentPortal.CourseCatalogService.DAL.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("Courses");
        b.HasKey(c => c.Id);
        b.Property(c => c.Code).IsRequired().HasMaxLength(50);
        b.HasIndex(c => c.Code).IsUnique();
        b.Property(c => c.Title).IsRequired().HasMaxLength(200);
        b.Property(c => c.Description).HasMaxLength(2000);
        b.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        b.Property(c => c.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

        b.HasOne(c => c.Instructor)
            .WithMany(u => u.CoursesAsInstructor)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(c => c.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
