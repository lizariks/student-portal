namespace StudentPortal.UnitTests.Controllers;

using StudentPortal.CourseCatalogService.APii.Controllers;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using FluentAssertions;

public class LessonsControllerTests
{
    private readonly Mock<ILessonService> _mockService;
    private readonly LessonsController _sut;

    public LessonsControllerTests()
    {
        _mockService = new Mock<ILessonService>();
        _sut = new LessonsController(_mockService.Object);
    }
    
    [Fact]
    public async Task GetLessonsByModule_Exists_ReturnsOk()
    {
        var lessons = new List<LessonDto> { new LessonDto { Id = 1, Title = "L1" } };
        _mockService.Setup(s => s.GetLessonsByModuleAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(lessons);

        var result = await _sut.GetLessonsByModule(1, default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateLesson_WhenBusinessException_ReturnsConflict()
    {
        // arrange
        var id = 1;
        var dto = new LessonUpdateDto { Title = "Update" };
        _mockService.Setup(s => s.UpdateLessonAsync(id, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException("Conflict in business rules"));

        // act
        var result = await _sut.UpdateLesson(id, dto, default);

        // assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }
    
    [Fact]
    public async Task ReorderLesson_InvalidOrder_ReturnsBadRequest()
    {
        // act
        var result = await _sut.ReorderLesson(1, 0, default);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}