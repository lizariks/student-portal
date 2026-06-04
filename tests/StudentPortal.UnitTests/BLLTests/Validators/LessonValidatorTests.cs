namespace StudentPortal.UnitTests.BLLTests.Validators;


using FluentValidation.TestHelper;
using StudentPortal.CourseCatalogService.BLL.Validators.Lessons;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using Xunit;

public class LessonValidatorTests
{
    private readonly LessonCreateDtoValidator _validator = new();

    [Theory]
    [InlineData("")]           
    [InlineData("Ab")]        
    [InlineData("Very long title that exceeds the limit of fifty characters for this lesson")] 
    public void LessonTitle_ShouldHaveError_WhenInvalid(string title)
    {
        // arrange
        var model = new LessonCreateDto 
        { 
            Title = title, 
            Content = "Valid content for the lesson", 
            ModuleId = 1,
            Order = 6
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]           
    [InlineData("Short")]     
    public void LessonContent_ShouldHaveError_WhenInvalid(string content)
    {
        // arrange
        var model = new LessonCreateDto 
        { 
            Title = "Valid Title", 
            Content = content, 
            ModuleId = 1,
            Order = 4
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void LessonModuleId_ShouldHaveError_WhenEmpty()
    {
        // arrange
        var model = new LessonCreateDto 
        { 
            Title = "Valid Title", 
            Content = "Valid content for the lesson", 
            ModuleId = 0,
            Order = 2
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.ModuleId);
    }

    [Fact]
    public void ValidModel_ShouldNotHaveErrors()
    {
        // arrange
        var model = new LessonCreateDto 
        { 
            Title = "Introduction to SQL", 
            Content = "In this lesson, we will cover the basics of SQL queries.", 
            ModuleId = 5,
            Order = 1
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}