namespace StudentPortal.CourseCatalogService.BLL.Validators.Materials;
using FluentValidation;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;

public class MaterialUpdateDtoValidator : AbstractValidator<MaterialUpdateDto>
{
    public MaterialUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Material title is required.")
            .MaximumLength(200).WithMessage("Material title cannot exceed 200 characters.")
            .MinimumLength(3).WithMessage("Material title must be at least 3 characters long.");

        RuleFor(x => x.Url)
            .MaximumLength(1000).WithMessage("Material URL cannot exceed 1000 characters.")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Material URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.Url));

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Material type is required.")
            .Must(type => new[] { "PDF", "Video", "Link", "Document", "Image", "Audio", "Archive" }.Contains(type))
            .WithMessage("Material type must be one of: PDF, Video, Link, Document, Image, Audio, Archive.");

        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Material order must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Material order cannot exceed 1000.");
    }
}