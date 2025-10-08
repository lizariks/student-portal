namespace StudentPortal.CourseCatalogService.BLL.Validators.Users;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Nickname cannot be empty.")
            .MaximumLength(50).WithMessage("Nickname cannot exceed 50 characters.")
            .MinimumLength(3).WithMessage("Nickname must be at least 3 characters long.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Nickname can only contain letters, numbers, underscores, and hyphens.")
            .When(x => x.Nickname != null);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name cannot be empty.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters long.")
            .Matches("^[a-zA-Z-' ]+$").WithMessage("First name can only contain letters, hyphens, apostrophes, and spaces.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name cannot be empty.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters long.")
            .Matches("^[a-zA-Z-' ]+$").WithMessage("Last name can only contain letters, hyphens, apostrophes, and spaces.")
            .When(x => x.LastName != null);

        RuleFor(x => x)
            .Must(dto => HasAtLeastOneProperty(dto))
            .WithMessage("At least one field must be provided for update.");
    }

    private bool HasAtLeastOneProperty(UserUpdateDto dto)
    {
        return dto.Nickname != null ||
               dto.FirstName != null ||
               dto.LastName != null;
    }
}