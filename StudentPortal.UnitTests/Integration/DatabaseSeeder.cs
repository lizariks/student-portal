namespace StudentPortal.UnitTests.Integration;

using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities;

public static class DatabaseSeeder
{
    public static void Seed(CourseCatalogDbContext db)
    {
        if (db.Courses.Any())
            return;

        var course = new Course
        {
            Id = 1,
            Code = "INT-101",
            Title = "Integration Test Course",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Courses.Add(course);
        db.SaveChanges();
    }
}