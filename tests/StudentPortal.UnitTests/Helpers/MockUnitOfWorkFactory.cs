namespace StudentPortal.UnitTests.Helpers;

using Moq;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;


public static class MockUnitOfWorkFactory
{
    public static (Mock<IUnitOfWork> UoW, Mock<ICourseRepository> Courses, Mock<ILessonRepository> Lessons) Create()
    {
        var mockCourseRepo = new Mock<ICourseRepository>();
        var mockLessonRepo = new Mock<ILessonRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

        mockUoW.Setup(u => u.Courses).Returns(mockCourseRepo.Object);
        mockUoW.Setup(u => u.Lessons).Returns(mockLessonRepo.Object);
        mockUoW.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (mockUoW, mockCourseRepo, mockLessonRepo);
    }
}