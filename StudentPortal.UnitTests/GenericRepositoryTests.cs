using StudentPortal.UnitTests.Helpers;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace StudentPortal.UnitTests;

public class GenericRepositoryTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly GenericRepository<Course> _sut;

    public GenericRepositoryTests()
    {
        _context = DbContextFactory.Create();
        _sut = new GenericRepository<Course>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_EntityExists_ReturnsEntity()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // act
        var result = await _sut.GetByIdAsync(1);

        // assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_EntityDoesNotExist_ReturnsNull()
    {
        // act
        var result = await _sut.GetByIdAsync(999);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_HasEntries_ReturnsList()
    {
        // arrange
        var courses = new List<Course> 
        { 
            TestDataBuilder.CreateCourse(1), 
            TestDataBuilder.CreateCourse(2), 
            TestDataBuilder.CreateCourse(3) 
        };
        _context.Courses.AddRange(courses);
        await _context.SaveChangesAsync();

        // act
        var result = await _sut.GetAllAsync();

        // assert
        result.Should().HaveCount(3);
        result.Should().NotContainNulls();
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyList()
    {
        // act
        var result = await _sut.GetAllAsync();

        // assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidEntity_AddsToTracker()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(1);

        // act
        await _sut.AddAsync(course);
        await _context.SaveChangesAsync();

        // assert
        _context.Courses.Count().Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_FieldChanged_PersistsChanges()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(1, "Original Title");
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        _context.Entry(course).State = EntityState.Detached;

        // act
        var entity = await _sut.GetByIdAsync(1, asNoTracking: false);
        entity!.Title = "Updated Title";
        await _sut.UpdateAsync(entity);
        await _context.SaveChangesAsync();

        // assert
        var result = await _sut.GetByIdAsync(1);
        result!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Delete_ExistingEntity_RemovesFromDatabase()
    {
        // arrange
        var course1 = TestDataBuilder.CreateCourse(1);
        var course2 = TestDataBuilder.CreateCourse(2);
        _context.Courses.AddRange(course1, course2);
        await _context.SaveChangesAsync();

        // act
        _sut.Delete(course1);
        await _context.SaveChangesAsync();

        // assert
        var all = await _sut.GetAllAsync();
        all.Should().HaveCount(1);
        var deleted = await _sut.GetByIdAsync(1);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistentEntity_ThrowsException()
    {
        // arrange
        var course = TestDataBuilder.CreateCourse(999);

        // act
        _sut.Delete(course);
        Func<Task> act = async () => await _context.SaveChangesAsync();

        // assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    public void Dispose() => _context.Dispose();
}