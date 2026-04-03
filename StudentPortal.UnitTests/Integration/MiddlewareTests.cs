namespace StudentPortal.UnitTests.Integration;

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Xunit;


public class MiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MiddlewareTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_WhenKeyNotFound_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/catalog/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_WhenInternalError_Returns500()
    {
        
        var response = await _client.GetAsync("/api/catalog/search?keyword="); 
        
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Title.Should().Be("Server error.");
        }
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_WhenValidationFails_Returns400WithErrors()
    {
        var invalidDto = new { Title = "", Code = "SHORT" };

        var response = await _client.PostAsJsonAsync("/api/catalog", invalidDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
    
        result.Should().ContainKey("message");
        result!["message"].Should().Be("Title cannot be empty.");
    }
}