using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.AddCourseReview;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.DeleteCourseReview;
using StudentPortal.DiscussionService.Application.Commands.CourseReviewCommands.UpdateCourseReview;
using StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewById;
using StudentPortal.DiscussionService.Application.Queries.CourseReviewQueries.GetCourseReviewsByTarget;
using StudentPortal.DiscussionService.Domain.Enums;

namespace StudentPortal.DiscussionService.API.Controllers;
    [Route("api/[controller]")]
    public class CourseReviewController : BaseApiController
    {
        public CourseReviewController(IMediator mediator) : base(mediator) { }

        // GET: api/coursereview/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
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

        // GET: api/coursereview/by-target/{targetId}
        [HttpGet("by-target/{targetId:guid}")]
        public async Task<IActionResult> GetByTarget(Guid targetId, [FromQuery] TargetType targetType, CancellationToken cancellationToken)
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

        // POST: api/coursereview
        [HttpPost]
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

        // PUT: api/coursereview/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseReviewCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.ReviewId != id)
                    return BadRequest("ID mismatch");

                // ETag concurrency check
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

        // DELETE: api/coursereview/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
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
