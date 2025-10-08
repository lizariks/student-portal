namespace StudentPortal.CourseCatalogService.BLL.Validators.Roles;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;
public class RoleUpdateDtoValidator : AbstractValidator<RoleUpdateDto>
{
    public RoleUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters long.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Role name can only contain letters, numbers, underscores, and hyphens.");
    }
}