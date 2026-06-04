using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.CreateComment;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.DeleteComment;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.UpdateComment;
using StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetCommentById;
using StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetComments;
using StudentPortal.DiscussionService.Domain.Parameters;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;


namespace StudentPortal.DiscussionService.API.Controllers
{
    [Route("api/[controller]")]
    public class CommentController : BaseApiController
    {
        public CommentController(IMediator mediator) : base(mediator) { }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _mediator.Send(new GetCommentByIdQuery { CommentId = id }, cancellationToken);
                if (comment == null)
                    return NotFound();

                var etag = GenerateETag(comment.UpdatedAt);
                AddETagHeader(etag);

                return Ok(comment);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments([FromQuery] CommentParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetCommentsQuery();
                var comments = await _mediator.Send(query, cancellationToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Create([FromBody] CreateCommentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var createdComment = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdComment.Id }, createdComment);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:guid}")]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCommentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.CommentId != id)
                    return BadRequest("ID mismatch");

                var requestETag = GetIfMatchHeader();
                var existingComment = await _mediator.Send(new GetCommentByIdQuery { CommentId = id }, cancellationToken);
                if (existingComment == null)
                    return NotFound();

                var currentETag = GenerateETag(existingComment.UpdatedAt);
                if (!ValidateETag(requestETag, currentETag))
                    return StatusCode(412, "ETag mismatch");

                var updatedComment = await _mediator.Send(command, cancellationToken);

                var newETag = GenerateETag(updatedComment.UpdatedAt);
                AddETagHeader(newETag);

                return Ok(updatedComment);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        [RequirePermission("discussion:delete")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteCommentCommand { CommentId = id };
                await _mediator.Send(command, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
