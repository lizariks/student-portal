using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StudentPortal.DiscussionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator _mediator;

    protected BaseApiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected IActionResult HandleException(Exception ex)
    {
        return ex switch
        {
            ArgumentException => BadRequest(ex.Message),
            KeyNotFoundException => NotFound(ex.Message),
            InvalidOperationException => Conflict(ex.Message),
            UnauthorizedAccessException => Forbid(),
            _ => StatusCode(500, "An unexpected error occurred.")
        };
    }

    protected string GenerateETag(DateTime timestamp)
    {
        return $"\"{timestamp.Ticks}\"";
    }

    protected void AddETagHeader(string etag)
    {
        Response.Headers.Add("ETag", etag);
    }

    protected string? GetIfMatchHeader()
    {
        return Request.Headers["If-Match"].FirstOrDefault();
    }

    protected bool ValidateETag(string? requestETag, string currentETag)
    {
        if (string.IsNullOrEmpty(requestETag))
            return true;

        return requestETag.Trim('"') == currentETag.Trim('"');
    }
}