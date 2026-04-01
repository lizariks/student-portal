using Microsoft.EntityFrameworkCore;
using StudentPortal.UnitTests.Helpers;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.UoW;
using FluentAssertions;
using System.Reflection;

namespace StudentPortal.UnitTests;

public class UnitOfWorkTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly UnitOfWork _uow;

    public UnitOfWorkTests()
    {
        _context = DbContextFactory.Create();
        _uow = new UnitOfWork(_context);
    }

    // checking if repos use the same context 
    [Fact]
    public void UnitOfWork_Repositories_ShareSameDbContext()
    {
        // act
        object? GetContext(object repository)
        {
            var type = repository.GetType();
            var field = type.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance) 
                        ?? type.BaseType?.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(repository);
        }

        var ctx1 = GetContext(_uow.Courses);
        var ctx2 = GetContext(_uow.Lessons);

        // assert
        ctx1.Should().NotBeNull();
        ctx1.Should().BeSameAs(ctx2);
        ctx1.Should().BeSameAs(_context);
    }
    
    [Fact]
    public async Task SaveChangesAsync_AfterMultipleAdds_PersistsAllEntities()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(10, "UoW Course");
        var lesson = TestDataBuilder.CreateLesson(10, "UoW Lesson", course.Id);

        // act
        await _uow.Courses.AddAsync(course);
        await _uow.Lessons.AddAsync(lesson);
        await _uow.SaveChangesAsync();

        // assert
        _context.Courses.Should().Contain(c => c.Id == 10);
        _context.Lessons.Should().Contain(l => l.Id == 10);
    }
    
    [Fact]
    public void Dispose_WhenCalled_DoesNotThrow()
    {
        // arrange
        var uow = new UnitOfWork(_context);

        // act and assert
        Action act = () => uow.Dispose();
        act.Should().NotThrow();
    }
 
    [Fact]
    public async Task SaveChangesAsync_ShouldCommitAllChanges()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(1, "New Course");
        await _uow.Courses.AddAsync(course);

        // act
        var affectedRows = await _uow.SaveChangesAsync();

        // assert
        affectedRows.Should().BeGreaterThan(0);
        var exists = await _context.Courses.AnyAsync(c => c.Id == 1);
        exists.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
        _uow.Dispose();
    }
}