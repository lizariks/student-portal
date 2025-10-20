using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ReopenDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;
using StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadById;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.API.Controllers;
    [Route("api/[controller]")]
    public class DiscussionThreadController : BaseApiController
    {
        public DiscussionThreadController(IMediator mediator) : base(mediator) { }

        // GET: api/discussionthread/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var thread = await _mediator.Send(new GetDiscussionThreadByIdQuery { ThreadId = id }, cancellationToken);
                if (thread == null)
                    return NotFound();

                var etag = GenerateETag(thread.UpdatedAt);
                AddETagHeader(etag);

                return Ok(thread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/discussionthread
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiscussionThreadCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var thread = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = thread.Id }, thread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // PUT: api/discussionthread/{id}/edit-comment
        [HttpPut("{id:guid}/edit-comment")]
        public async Task<IActionResult> EditComment(Guid id, [FromBody] EditDiscussionThreadCommentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.ThreadId != id)
                    return BadRequest("Thread ID mismatch");

                var existingThread = await _mediator.Send(new GetDiscussionThreadByIdQuery { ThreadId = id }, cancellationToken);
                if (existingThread == null)
                    return NotFound();

                var requestETag = GetIfMatchHeader();
                var currentETag = GenerateETag(existingThread.UpdatedAt);
                if (!ValidateETag(requestETag, currentETag))
                    return StatusCode(412, "ETag mismatch");

                var updatedThread = await _mediator.Send(command, cancellationToken);

                AddETagHeader(GenerateETag(updatedThread.UpdatedAt));

                return Ok(updatedThread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/discussionthread/{id}/add-comment
        [HttpPost("{id:guid}/add-comment")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentToThreadCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.ThreadId != id)
                    return BadRequest("Thread ID mismatch");

                var updatedThread = await _mediator.Send(command, cancellationToken);
                return Ok(updatedThread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/discussionthread/{id}/close
        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id, [FromBody] UserInfo actor, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CloseDiscussionThreadCommand(id, actor);
                await _mediator.Send(command, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/discussionthread/{id}/reopen
        [HttpPost("{id:guid}/reopen")]
        public async Task<IActionResult> Reopen(Guid id, [FromBody] UserInfo actor, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ReopenDiscussionThreadCommand { ThreadId = id, Actor = actor };
                var thread = await _mediator.Send(command, cancellationToken);
                return Ok(thread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // POST: api/discussionthread/{id}/resolve-comment
        [HttpPost("{id:guid}/resolve-comment")]
        public async Task<IActionResult> ResolveComment(Guid id, [FromBody] ResolveDiscussionThreadCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.ThreadId != id)
                    return BadRequest("Thread ID mismatch");

                var thread = await _mediator.Send(command, cancellationToken);
                return Ok(thread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
