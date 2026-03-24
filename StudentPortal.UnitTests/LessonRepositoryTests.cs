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

    // Позитивний сценарій: Отримання уроків по ModuleId
    [Fact]
    public async Task GetLessonsByModuleAsync_ReturnsCorrectLessons()
    {
        // Arrange
        int targetModuleId = 10;
        _context.Lessons.AddRange(new List<Lesson> {
            new Lesson { Id = 1, ModuleId = targetModuleId, Title = "L1", Order = 1 },
            new Lesson { Id = 2, ModuleId = 20, Title = "L2" }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetLessonsByModuleAsync(targetModuleId);

        // Assert
        Assert.Single(result);
        Assert.Equal(targetModuleId, result.First().ModuleId);
    }

    // Порожній результат: Модуль існує, але в ньому немає уроків
    [Fact]
    public async Task GetLessonsByModuleAsync_WhenModuleEmpty_ReturnsEmptyList()
    {
        // Act
        var result = await _repo.GetLessonsByModuleAsync(999);

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetLessonsByDurationRangeAsync_ExactBoundaries_ReturnsMatches()
    {
        // Arrange
        var min = TimeSpan.FromMinutes(10);
        var max = TimeSpan.FromMinutes(20);
    
        // 1. Створюємо модуль (бо репозиторій робить .Include(l => l.Module))
        var testModule = new Module { Id = 1, Title = "Test Module" };
        _context.Modules.Add(testModule);

        // 2. Створюємо урок і прив'язуємо його до модуля
        _context.Lessons.Add(new Lesson 
        { 
            Id = 1, 
            Title = "Boundary Lesson",
            ModuleId = testModule.Id, // Прив'язка
            EstimatedDuration = min 
        }); 
    
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetLessonsByDurationRangeAsync(min, max);

        // Assert
        Assert.Single(result); // Тепер тут має бути 1
    }

    public void Dispose() => _context.Dispose();
}