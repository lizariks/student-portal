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
        // act 
        var response = await _client.GetAsync("/api/catalog/1");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var course = await response.Content.ReadFromJsonAsync<CourseDetailsDto>();
        course.Should().NotBeNull();
        course!.Title.Should().Be("Integration Test Course");
    }

    [Fact]
    public async Task GetCourseById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // act 
        var response = await _client.GetAsync("/api/catalog/999");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCourse_ReturnsBadRequest_WhenTitleIsEmpty()
    {
        // arrange 
        var invalidCourse = new CourseCreateDto { Title = "", Code = "ERR" };

        // act
        var response = await _client.PostAsJsonAsync("/api/catalog", invalidCourse);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}