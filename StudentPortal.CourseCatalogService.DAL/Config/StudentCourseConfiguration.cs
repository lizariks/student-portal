namespace StudentPortal.CourseCatalogService.DAL.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.CourseCatalogService.Domain.Entities;

public class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> b)
    {
        b.ToTable("StudentCourses");
        b.HasKey(sc => new { sc.UserId, sc.CourseId });
        b.Property(sc => sc.EnrolledAt).HasDefaultValueSql("GETUTCDATE()");
    }
}