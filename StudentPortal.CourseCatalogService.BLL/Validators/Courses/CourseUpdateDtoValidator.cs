namespace StudentPortal.CourseCatalogService.BLL.Validators.Courses;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
public class CourseUpdateDtoValidator : AbstractValidator<CourseUpdateDto>
{
    public CourseUpdateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters.")
            .MinimumLength(2).WithMessage("Course code must be at least 2 characters long.")
            .Matches("^[A-Z0-9-]+$").WithMessage("Course code must contain only uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.")
            .MinimumLength(3).WithMessage("Course title must be at least 3 characters long.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Course description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.IsPublished)
            .NotNull().WithMessage("IsPublished field is required.");

        RuleFor(x => x.InstructorId)
            .GreaterThan(0).WithMessage("Instructor ID must be greater than 0.")
            .When(x => x.InstructorId.HasValue);
    }
}