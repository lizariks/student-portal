namespace StudentPortal.UnitTests.BLLTests.Validators;

using FluentValidation.TestHelper;
using StudentPortal.CourseCatalogService.BLL.Validators.Courses;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;


public class CourseValidatorTests
{
    private readonly CourseCreateDtoValidator _validator = new();

    [Theory]
    [InlineData("")]  //null       
    [InlineData("A")]    // too short   
    [InlineData("CS_101")]   // we cant put that symbol(_)
    public void CourseCode_ShouldHaveError_WhenInvalid(string code)
    {
        var model = new CourseCreateDto { Code = code, Title = "Valid Title" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ValidModel_ShouldNotHaveErrors()
    {
        var model = new CourseCreateDto { Code = "MATH-101", Title = "Mathematics", IsPublished = true };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}