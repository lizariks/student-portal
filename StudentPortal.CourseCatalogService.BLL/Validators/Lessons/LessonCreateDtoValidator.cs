namespace StudentPortal.CourseCatalogService.BLL.Validators.Lessons;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
public class LessonCreateDtoValidator : AbstractValidator<LessonCreateDto>
{
    public LessonCreateDtoValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Module ID is required.")
            .GreaterThan(0).WithMessage("Module ID must be greater than 0.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lesson title is required.")
            .MaximumLength(200).WithMessage("Lesson title cannot exceed 200 characters.")
            .MinimumLength(3).WithMessage("Lesson title must be at least 3 characters long.");

        RuleFor(x => x.Content)
            .MaximumLength(10000).WithMessage("Lesson content cannot exceed 10,000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Lesson order must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Lesson order cannot exceed 1000.");

        RuleFor(x => x.EstimatedDuration)
            .Must(duration => duration.HasValue && duration.Value > TimeSpan.Zero)
            .WithMessage("Estimated duration must be greater than zero.")
            .Must(duration => duration.HasValue && duration.Value <= TimeSpan.FromHours(24))
            .WithMessage("Estimated duration cannot exceed 24 hours.")
            .When(x => x.EstimatedDuration.HasValue);
    }
}

