using Microsoft.EntityFrameworkCore;
using StudentPortal.UnitTests.Helpers;

namespace StudentPortal.UnitTests;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.UoW;

public class UnitOfWorkTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly UnitOfWork _uow;

    public UnitOfWorkTests()
    {
        _context = DbContextFactory.Create();
        _uow = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldCommitAllChanges()
    {
        // Arrange
        var course = new Course { Id = 1, Title = "New Course", Code = "NC1" };
        await _uow.Courses.AddAsync(course);

        // Act
        var affectedRows = await _uow.SaveChangesAsync();

        // Assert
        Assert.True(affectedRows > 0);
        var exists = await _context.Courses.AnyAsync(c => c.Id == 1);
        Assert.True(exists);
    }

    public void Dispose() => _uow.Dispose();
}