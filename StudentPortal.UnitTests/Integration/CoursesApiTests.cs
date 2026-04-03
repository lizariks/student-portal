namespace StudentPortal.UnitTests.Integration;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using Xunit;


public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCourseById_ReturnsSuccessAndCorrectData()
    {
        // Act (Крок 6)
        var response = await _client.GetAsync("/api/catalog/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var course = await response.Content.ReadFromJsonAsync<CourseDetailsDto>();
        course.Should().NotBeNull();
        course!.Title.Should().Be("Integration Test Course");
    }

    [Fact]
    public async Task GetCourseById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // Act (Крок 7)
        var response = await _client.GetAsync("/api/catalog/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCourse_ReturnsBadRequest_WhenTitleIsEmpty()
    {
        // Arrange (Крок 8)
        var invalidCourse = new CourseCreateDto { Title = "", Code = "ERR" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog", invalidCourse);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}