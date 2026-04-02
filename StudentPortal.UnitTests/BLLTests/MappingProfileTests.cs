using AutoMapper;
using FluentAssertions;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Mapping;
using StudentPortal.CourseCatalogService.Domain.Entities;
using Xunit;
using Microsoft.Extensions.Logging;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;

namespace StudentPortal.BLL.Tests.Mappings;

public class MappingProfileTests
{
    private readonly IConfigurationProvider _config;
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var loggerFactory = new LoggerFactory();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(CourseProfile).Assembly);
        }, loggerFactory);

        _mapper = config.CreateMapper();
        _config = config;
    }

    [Fact]
    public void MappingProfile_ConfigurationIsValid()
    {
        _config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CourseToCourseDto_AllFieldsCorrectlyMapped()
    {
        // arrange
        var source = new Course 
        { 
            Id = 5, 
            Title = "C# Advanced", 
            Code = "CS-401", 
            IsPublished = true 
        };

        // act
        var result = _mapper.Map<CourseDto>(source);

        // assert
        result.Id.Should().Be(source.Id);
        result.Title.Should().Be(source.Title);
        result.Code.Should().Be(source.Code);
        result.IsPublished.Should().Be(source.IsPublished);
    }

    [Fact]
    public void Map_CourseCreateDtoToCourse_IdNotMapped()
    {
        // arrange
        var dto = new CourseCreateDto { Title = "New", Code = "NEW-1" };

        // act
        var result = _mapper.Map<Course>(dto);

        // Assert
        result.Title.Should().Be(dto.Title);
        result.Id.Should().Be(0); 
    }
    [Fact]
    public void Map_LessonToLessonDto_AllFieldsCorrectlyMapped()
    {
        // arrange
        var source = new Lesson 
        { 
            Id = 10, 
            Title = "Introduction to SQL", 
            Content = "Basic SELECT queries",
            ModuleId = 1 
        };

        // act
        var result = _mapper.Map<LessonDto>(source);

        // assert
        result.Id.Should().Be(source.Id);
        result.Title.Should().Be(source.Title);
    }

    [Fact]
    public void Map_LessonCreateDtoToLesson_RelationsNotMapped()
    {
        // arrange
        var dto = new LessonCreateDto 
        { 
            Title = "NoSQL Basics", 
            Content = "Flexible schemas",
            ModuleId = 2
        };

        // act
        var result = _mapper.Map<Lesson>(dto);

        // assert
        result.Title.Should().Be(dto.Title);
        result.Content.Should().Be(dto.Content);
        result.ModuleId.Should().Be(dto.ModuleId);
        
        result.Module.Should().BeNull();
        result.Id.Should().Be(0);
    }
}