using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.CreateComment;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.DeleteComment;
using StudentPortal.DiscussionService.Application.Commands.CommentCommands.UpdateComment;
using StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetCommentById;
using StudentPortal.DiscussionService.Application.Queries.CommentQueries.GetComments;
using StudentPortal.DiscussionService.Domain.Parameters;


namespace StudentPortal.DiscussionService.API.Controllers
{
    [Route("api/[controller]")]
    public class CommentController : BaseApiController
    {
        public CommentController(IMediator mediator) : base(mediator) { }

        // GET: api/comment/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
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

        // GET: api/comment
        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] CommentParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetCommentsQuery(); // You can extend this query to accept parameters if needed
                var comments = await _mediator.Send(query, cancellationToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/comment
        [HttpPost]
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

        // PUT: api/comment/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Ensure the command targets the correct ID
                if (command.CommentId != id)
                    return BadRequest("ID mismatch");

                // ETag concurrency check
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

        // DELETE: api/comment/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
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
