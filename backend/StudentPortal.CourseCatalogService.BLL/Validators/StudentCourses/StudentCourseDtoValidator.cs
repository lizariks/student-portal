namespace StudentPortal.CourseCatalogService.BLL.Validators.StudentCourses;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
public class StudentCourseCreateDtoValidator : AbstractValidator<StudentCourseCreateDto>
{
    public StudentCourseCreateDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .GreaterThan(0).WithMessage("Course ID must be greater than 0.");
    }
}