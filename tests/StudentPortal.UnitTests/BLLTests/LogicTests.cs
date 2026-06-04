namespace StudentPortal.UnitTests.BLLTests;

using Moq;
using StudentPortal.UnitTests.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities;


public class LogicTests
{
    [Fact]
    public async Task CreateCourse_ShouldVerifyRepositoryCall()
    {
        // arrange
        var (mockUoW, mockRepo, _) = MockUnitOfWorkFactory.Create();
        var course = new Course { Title = "New Course" };

        // act
        await mockRepo.Object.AddAsync(course);
        await mockUoW.Object.SaveChangesAsync();

        // assert
        mockRepo.Verify(x => x.AddAsync(It.IsAny<Course>(), default), Times.Once);
        mockUoW.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}