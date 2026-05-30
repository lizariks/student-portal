using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.AddCommentToThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.DeleteComment;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ReopenDiscussionThread;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.ResolveDiscussionThread;
using StudentPortal.DiscussionService.Application.DTOs;
using StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadById;
using StudentPortal.DiscussionService.Application.Queries.DiscussionThreadQueries.GetDiscussionThreadsByTarget;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace StudentPortal.DiscussionService.API.Controllers;
    [Route("api/[controller]")]
    public class DiscussionThreadController : BaseApiController
    {
        private readonly ILogger<DiscussionThreadController> _logger;

        public DiscussionThreadController(IMediator mediator, ILogger<DiscussionThreadController> logger) : base(mediator)
        {
            _logger = logger;
        }

        [HttpGet("by-target")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTarget(
            [FromQuery] string targetId,
            [FromQuery] TargetType targetType,
            CancellationToken cancellationToken)
        {
            try
            {
                var threads = await _mediator.Send(
                    new GetDiscussionThreadsByTargetQuery { TargetId = targetId, TargetType = targetType },
                    cancellationToken);
                return Ok(threads);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateDiscussionThreadCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var thread = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = thread.Id }, thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create: {ExType} - {ExMsg}", ex.GetType().Name, ex.Message);
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/edit-comment")]
        [AllowAnonymous]
        public async Task<IActionResult> EditComment(string id, [FromBody] EditDiscussionThreadCommentCommand command, CancellationToken cancellationToken)
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

        [HttpPost("{id}/add-comment")]
        [AllowAnonymous]
        public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentToThreadCommand command, CancellationToken cancellationToken)
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
                _logger.LogError(ex, "Error in AddComment: {ExType} - {ExMsg}", ex.GetType().Name, ex.Message);
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}/comments/{commentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteComment(string id, string commentId, [FromBody] UserInfoRequest actor, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteCommentCommand { ThreadId = id, CommentId = commentId, Actor = actor };
                var updatedThread = await _mediator.Send(command, cancellationToken);
                return Ok(updatedThread);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{id}/close")]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Close(string id, [FromBody] UserInfo actor, CancellationToken cancellationToken)
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

        [HttpPost("{id}/reopen")]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Reopen(string id, [FromBody] UserInfo actor, CancellationToken cancellationToken)
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

        [HttpPost("{id}/resolve-comment")]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> ResolveComment(string id, [FromBody] ResolveDiscussionThreadCommand command, CancellationToken cancellationToken)
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
