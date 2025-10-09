namespace StudentPortal.CourseCatalogService.APii.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }
        

        /// <summary>
        /// Get module by ID with lessons
        /// </summary>
        /// <param name="id">Module identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Module with lessons</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ModuleDto>> GetModuleWithLessons(int id, CancellationToken cancellationToken)
        {
            var module = await _moduleService.GetModuleWithLessonsAsync(id, cancellationToken);
            if (module == null)
                return NotFound(new { message = $"Module with ID {id} not found." });

            return Ok(module);
        }

        /// <summary>
        /// Get modules by course ID
        /// </summary>
        /// <param name="courseId">Course identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of modules in the course</returns>
        [HttpGet("course/{courseId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ModuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModulesByCourse(
            int courseId,
            CancellationToken cancellationToken)
        {
            var modules = await _moduleService.GetModulesByCourseAsync(courseId, cancellationToken);
            return Ok(modules);
        }

        /// <summary>
        /// Create a new module
        /// </summary>
        /// <param name="dto">Module data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created module</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ModuleDto>> CreateModule(
            [FromBody] ModuleDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var module = await _moduleService.CreateModuleAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetModuleWithLessons), new { id = module.Id }, module);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing module
        /// </summary>
        /// <param name="id">Module identifier</param>
        /// <param name="dto">Module update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated module</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ModuleDto>> UpdateModule(
            int id,
            [FromBody] ModuleDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in body." });

            try
            {
                var module = await _moduleService.UpdateModuleAsync(dto, cancellationToken);
                return Ok(module);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a module
        /// </summary>
        /// <param name="id">Module identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteModule(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _moduleService.DeleteModuleAsync(id, cancellationToken);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }