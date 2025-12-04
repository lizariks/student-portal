namespace StudentPortal.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.BLL.DTOs;
using StudentPortal.Enrollment.Domain.Entities;
using StudentPortal.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;

    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentStatusHistoryController : ControllerBase
    {
        private readonly IEnrollmentStatusHistoryService _service;
        public EnrollmentStatusHistoryController(IEnrollmentStatusHistoryService service)
            => _service = service;

        [HttpGet("by-enrollment/{enrollmentId}")]
        [RequirePermission("enrollment:read")]
        public async Task<ActionResult<IEnumerable<EnrollmentStatusHistory>>> GetByEnrollment(int enrollmentId)
            => Ok(await _service.GetByEnrollmentAsync(enrollmentId));

        [HttpPost]
        [RequirePermission("enrollment:write")]
        public async Task<ActionResult<EnrollmentStatusHistory>> AddStatus([FromBody] EnrollmentStatusHistoryUpdateDto dto)
        {
            var status = await _service.AddStatusAsync(dto);
            return CreatedAtAction(nameof(GetByEnrollment), new { enrollmentId = status.EnrollmentId }, status);
        }
    }
