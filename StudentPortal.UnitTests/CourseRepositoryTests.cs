using StudentPortal.UnitTests.Helpers;

namespace StudentPortal.UnitTests;

using Xunit;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.Data;


public class CourseRepositoryTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly CourseRepository _repo;

    public CourseRepositoryTests()
    {
        _context = DbContextFactory.Create();
        _repo = new CourseRepository(_context);
    }

    // Позитивний сценарій: Пошук опублікованих курсів
    [Fact]
    public async Task GetPublishedCoursesAsync_ReturnsOnlyPublished()
    {
        // Arrange
        _context.Courses.AddRange(new List<Course> {
            new Course { Id = 1, Title = "C1", Code = "CODE-1", IsPublished = true },
            new Course { Id = 2, Title = "C2", Code = "CODE-2", IsPublished = false }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetPublishedCoursesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsPublished);
    }

    // Порожній результат: Пошук за ключовим словом, якого немає
    [Fact]
    public async Task SearchCoursesAsync_WhenNoMatch_ReturnsEmpty()
    {
        // Arrange
        _context.Courses.Add(new Course { Id = 1, Title = "Math", Code = "M1" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.SearchCoursesAsync("Physics");

        // Assert
        Assert.Empty(result);
    }

    // Граничний випадок: Пошук з порожнім рядком (null або пустий)
    [Fact]
    public async Task SearchCoursesAsync_WithEmptyKeyword_ReturnsEmpty()
    {
        // Act
        var result = await _repo.SearchCoursesAsync("");

        // Assert
        Assert.Empty(result);
    }

    public void Dispose() => _context.Dispose();
}