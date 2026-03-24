namespace StudentPortal.UnitTests.Helpers;

using StudentPortal.CourseCatalogService.Domain.Entities;

using StudentPortal.CourseCatalogService.Domain.Entities;

public static class TestDataBuilder
{
    // Додаємо Code, бо він обов'язковий для Course
    public static Course CreateCourse(int id = 1, string title = "Test Course", string code = "TEST-101") =>
        new Course 
        { 
            Id = id, 
            Title = title, 
            Code = code, // Обов'язкове поле
            IsPublished = true, 
            CreatedAt = DateTime.UtcNow 
        };

    // Додаємо Title, бо він обов'язковий для Lesson
    public static Lesson CreateLesson(int id = 1, string title = "Test Lesson", int moduleId = 1) =>
        new Lesson 
        { 
            Id = id, 
            Title = title, // Обов'язкове поле
            ModuleId = moduleId,
            Order = 1,
            EstimatedDuration = TimeSpan.FromMinutes(15)
        };
}