namespace StudentPortal.UnitTests.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentPortal.CourseCatalogService.APii.Controllers;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using FluentAssertions;


public class CoursesControllerTests
{
    private readonly Mock<ICourseService> _mockService;
    private readonly CoursesController _sut;

    public CoursesControllerTests()
    {
        _mockService = new Mock<ICourseService>();
        _sut = new CoursesController(_mockService.Object);
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }


    [Fact]
    public async Task GetCourseById_WhenExists_ReturnsOk()
    {
        // arrange
        var courseId = 1;
        var courseDto = new CourseDetailsDto { Id = courseId, Title = "Test Course" };
        _mockService.Setup(s => s.GetCourseByIdAsync(courseId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(courseDto);

        // act
        var result = await _sut.GetCourseById(courseId, default);

        // assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(courseDto);
    }

    [Fact]
    public async Task GetCourseById_WhenNotFound_ReturnsNotFound()
    {
        // arrange
        var courseId = 999;
        _mockService.Setup(s => s.GetCourseByIdAsync(courseId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException("Course not found"));

        // act
        var result = await _sut.GetCourseById(courseId, default);

        //assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }


    [Fact]
    public async Task CreateCourse_ValidModel_ReturnsCreated()
    {
        // arrange
        var dto = new CourseCreateDto { Title = "New", Code = "NW-1" };
        var createdCourse = new CourseDto { Id = 1, Title = "New", Code = "NW-1" };
        _mockService.Setup(s => s.CreateCourseAsync(dto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdCourse);

        // act
        var result = await _sut.CreateCourse(dto, default);

        // assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.RouteValues!["id"].Should().Be(createdCourse.Id);
        _mockService.Verify(s => s.CreateCourseAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCourse_InvalidModelState_ReturnsBadRequest()
    {
        // arrange
        _sut.ModelState.AddModelError("Title", "Required");
        var dto = new CourseCreateDto();

        // act
        var result = await _sut.CreateCourse(dto, default);

        // assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.CreateCourseAsync(It.IsAny<CourseCreateDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task DeleteCourse_WhenSuccess_ReturnsNoContent()
    {
        // act
        var result = await _sut.DeleteCourse(1, default);

        // assert
        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(s => s.DeleteCourseAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}