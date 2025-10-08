namespace StudentPortal.CourseCatalogService.BLL.Validators.Users;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Nickname is required.")
            .MaximumLength(50).WithMessage("Nickname cannot exceed 50 characters.")
            .MinimumLength(3).WithMessage("Nickname must be at least 3 characters long.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Nickname can only contain letters, numbers, underscores, and hyphens.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters long.")
            .Matches("^[a-zA-Z-' ]+$").WithMessage("First name can only contain letters, hyphens, apostrophes, and spaces.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters long.")
            .Matches("^[a-zA-Z-' ]+$").WithMessage("Last name can only contain letters, hyphens, apostrophes, and spaces.");
    }
}

