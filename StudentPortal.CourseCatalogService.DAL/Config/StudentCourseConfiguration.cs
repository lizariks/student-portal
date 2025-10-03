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

        b.Property(sc => sc.EnrolledAt)
            .HasDefaultValueSql("NOW()");

        b.HasOne(sc => sc.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(sc => sc.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}