namespace StudentPortal.CourseCatalogService.BLL.Validators.Modules;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
public class ModuleCreateDtoValidator : AbstractValidator<ModuleCreateDto>
{
    public ModuleCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Module title is required.")
            .MaximumLength(200).WithMessage("Module title cannot exceed 200 characters.")
            .MinimumLength(3).WithMessage("Module title must be at least 3 characters long.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Module description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Module order must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Module order cannot exceed 1000.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .GreaterThan(0).WithMessage("Course ID must be greater than 0.");
    }
}

