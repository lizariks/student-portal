using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.DeleteCourseReview;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.UpdateCourseReview;
using StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewById;
using StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewsByTarget;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace StudentPortal.DiscussionService.API.Controllers;
    [Route("api/[controller]")]
    public class CourseReviewController : BaseApiController
    {
        public CourseReviewController(IMediator mediator) : base(mediator) { }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var review = await _mediator.Send(new GetCourseReviewByIdQuery { ReviewId = id }, cancellationToken);
                if (review == null)
                    return NotFound();

                var etag = GenerateETag(review.UpdatedAt);
                AddETagHeader(etag);

                return Ok(review);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-target/{targetId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTarget(string targetId, [FromQuery] TargetType targetType, CancellationToken cancellationToken)
        {
            try
            {
                var reviews = await _mediator.Send(new GetCourseReviewsByTargetQuery
                {
                    TargetId = targetId,
                    TargetType = targetType
                }, cancellationToken);

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Create([FromBody] AddCourseReviewCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var createdReview = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdReview.Id }, createdReview);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:guid}")]
        [RequirePermission("discussion:write")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCourseReviewCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.ReviewId != id)
                    return BadRequest("ID mismatch");

                var requestETag = GetIfMatchHeader();
                var existingReview = await _mediator.Send(new GetCourseReviewByIdQuery { ReviewId = id }, cancellationToken);
                if (existingReview == null)
                    return NotFound();

                var currentETag = GenerateETag(existingReview.UpdatedAt);
                if (!ValidateETag(requestETag, currentETag))
                    return StatusCode(412, "ETag mismatch");

                var updatedReview = await _mediator.Send(command, cancellationToken);

                var newETag = GenerateETag(updatedReview.UpdatedAt);
                AddETagHeader(newETag);

                return Ok(updatedReview);
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
                var command = new DeleteCourseReviewCommand { ReviewId = id };
                var result = await _mediator.Send(command, cancellationToken);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
