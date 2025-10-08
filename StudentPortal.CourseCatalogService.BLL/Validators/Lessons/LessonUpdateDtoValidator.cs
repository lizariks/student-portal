namespace StudentPortal.CourseCatalogService.BLL.Validators.Lessons;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
public class LessonUpdateDtoValidator : AbstractValidator<LessonUpdateDto>
{
    public LessonUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lesson title cannot be empty.")
            .MaximumLength(200).WithMessage("Lesson title cannot exceed 200 characters.")
            .MinimumLength(3).WithMessage("Lesson title must be at least 3 characters long.")
            .When(x => x.Title != null);

        RuleFor(x => x.Content)
            .MaximumLength(10000).WithMessage("Lesson content cannot exceed 10,000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Lesson order must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Lesson order cannot exceed 1000.")
            .When(x => x.Order.HasValue);

        RuleFor(x => x.EstimatedDuration)
            .Must(duration => duration > TimeSpan.Zero)
            .WithMessage("Estimated duration must be greater than zero.")
            .Must(duration => duration <= TimeSpan.FromHours(24))
            .WithMessage("Estimated duration cannot exceed 24 hours.")
            .When(x => x.EstimatedDuration.HasValue);

        RuleFor(x => x)
            .Must(dto => HasAtLeastOneProperty(dto))
            .WithMessage("At least one field must be provided for update.");
    }

    private bool HasAtLeastOneProperty(LessonUpdateDto dto)
    {
        return dto.Title != null ||
               dto.Content != null ||
               dto.Order.HasValue ||
               dto.EstimatedDuration.HasValue;
    }
}
