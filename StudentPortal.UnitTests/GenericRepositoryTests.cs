using StudentPortal.UnitTests.Helpers;

namespace StudentPortal.UnitTests;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
public class GenericRepositoryTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly GenericRepository<Course> _sut; // sut - System Under Test

    public GenericRepositoryTests()
    {
        _context = DbContextFactory.Create();
        _sut = new GenericRepository<Course>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var course = TestDataBuilder.CreateCourse(1, "Generic Course", "GEN-01");
        // Act
        await _sut.AddAsync(course);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Courses.FindAsync(course.Id);
        Assert.NotNull(result);
        Assert.Equal(course.Title, result.Title);
    }

    public void Dispose() => _context.Dispose();
}