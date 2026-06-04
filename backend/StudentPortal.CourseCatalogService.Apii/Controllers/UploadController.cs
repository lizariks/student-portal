using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentPortal.CourseCatalogService.Apii.Controllers;

[ApiController]
[Route("api/upload")]
[AllowAnonymous]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"Only image files are allowed ({string.Join(", ", AllowedExtensions)}).");

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new { url = $"/uploads/{fileName}" });
    }
}
