using StudentPortal.UnitTests.Helpers;

namespace StudentPortal.UnitTests;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.Domain.Entities;
public class LessonRepositoryTests : IDisposable
{
    private readonly CourseCatalogDbContext _context;
    private readonly LessonRepository _repo;

    public LessonRepositoryTests()
    {
        _context = DbContextFactory.Create();
        _repo = new LessonRepository(_context);
    }
    
    [Fact]
    public async Task GetLessonWithMaterialsExplicitAsync_ShouldLoadMaterials()
    {
        // arrange
        var lesson = new Lesson { Id = 5, Title = "Lesson with Materials", ModuleId = 5};
        var material = new Material { Id = 1, Title = "PDF Guide", LessonId = 5 };
    
        _context.Lessons.Add(lesson);
        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        // act
        var result = await _repo.GetLessonWithMaterialsExplicitAsync(5);

        // assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Materials); //checking if materials connected
    }

// empty result for filters
    [Fact]
    public async Task GetLessonsWithoutMaterialsAsync_WhenAllHaveMaterials_ReturnsEmpty()
    {
        // arrange
        var lesson = new Lesson { Id = 10, Title = "Has Materials", ModuleId = 10};
        lesson.Materials.Add(new Material { Id = 2, Title = "Video" });
    
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        // act
        var result = await _repo.GetLessonsWithoutMaterialsAsync();

        // assert
        Assert.Empty(result); // must be empty
    }
    
    [Fact]
    public async Task GetOrderedLessonsInModuleAsync_ShouldReturnCorrectOrder()
    {
        // arrange
        int moduleId = 50;
        _context.Lessons.AddRange(new List<Lesson> {
            new Lesson { Id = 1, ModuleId = moduleId, Title = "Second", Order = 2 },
            new Lesson { Id = 2, ModuleId = moduleId, Title = "First", Order = 1 }
        });
        await _context.SaveChangesAsync();

        // act
        var result = (await _repo.GetOrderedLessonsInModuleAsync(moduleId)).ToList();

        // assert
        Assert.Equal("First", result[0].Title); 
        Assert.Equal("Second", result[1].Title); 
    }
    public void Dispose() => _context.Dispose();
}