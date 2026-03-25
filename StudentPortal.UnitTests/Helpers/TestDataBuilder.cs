namespace StudentPortal.UnitTests.Helpers;

using StudentPortal.CourseCatalogService.Domain.Entities;

using StudentPortal.CourseCatalogService.Domain.Entities;

public static class TestDataBuilder
{
    public static Course CreateCourse(int id = 1, string title = "Test Course", string code = "TEST-101") =>
        new Course 
        { 
            Id = id, 
            Title = title, 
            Code = code, 
            IsPublished = true, 
            CreatedAt = DateTime.UtcNow 
        };

    public static Lesson CreateLesson(int id = 1, string title = "Test Lesson", int moduleId = 1) =>
        new Lesson 
        { 
            Id = id, 
            Title = title, 
            ModuleId = moduleId,
            Order = 1,
            EstimatedDuration = TimeSpan.FromMinutes(15)
        };
}