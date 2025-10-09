namespace StudentPortal.CourseCatalogService.APii.Controllers;

using Microsoft.AspNetCore.Mvc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;


    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>
        /// Get all materials with pagination
        /// </summary>
        /// <param name="parameters">Pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paged list of materials</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedList<MaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedList<MaterialDto>>> GetPagedMaterials(
            [FromQuery] MaterialParameters parameters,
            CancellationToken cancellationToken)
        {
            var materials = await _materialService.GetPagedMaterialsAsync(parameters, cancellationToken);
            return Ok(materials);
        }

        /// <summary>
        /// Get material by ID with lesson details
        /// </summary>
        /// <param name="id">Material identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Material with lesson details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MaterialDto>> GetMaterialWithLesson(int id, CancellationToken cancellationToken)
        {
            var material = await _materialService.GetMaterialWithLessonAsync(id, cancellationToken);
            if (material == null)
                return NotFound(new { message = $"Material with ID {id} not found." });

            return Ok(material);
        }

        /// <summary>
        /// Get materials by lesson ID
        /// </summary>
        /// <param name="lessonId">Lesson identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of materials in the lesson</returns>
        [HttpGet("lesson/{lessonId:int}")]
        [ProducesResponseType(typeof(IEnumerable<MaterialDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsByLesson(
            int lessonId,
            CancellationToken cancellationToken)
        {
            var materials = await _materialService.GetMaterialsByLessonAsync(lessonId, cancellationToken);
            return Ok(materials);
        }

        /// <summary>
        /// Get ordered materials by lesson ID
        /// </summary>
        /// <param name="lessonId">Lesson identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ordered list of materials in the lesson</returns>
        [HttpGet("lesson/{lessonId:int}/ordered")]
        [ProducesResponseType(typeof(IEnumerable<MaterialDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetOrderedMaterialsByLesson(
            int lessonId,
            CancellationToken cancellationToken)
        {
            var materials = await _materialService.GetOrderedMaterialsByLessonAsync(lessonId, cancellationToken);
            return Ok(materials);
        }

        /// <summary>
        /// Get materials by type
        /// </summary>
        /// <param name="type">Material type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of materials of the specified type</returns>
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(IEnumerable<MaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsByType(
            MaterialType type,
            CancellationToken cancellationToken)
        {
            var materials = await _materialService.GetMaterialsByTypeAsync(type, cancellationToken);
            return Ok(materials);
        }

        /// <summary>
        /// Get materials without URL
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of materials without URL</returns>
        [HttpGet("without-url")]
        [ProducesResponseType(typeof(IEnumerable<MaterialDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsWithoutUrl(CancellationToken cancellationToken)
        {
            var materials = await _materialService.GetMaterialsWithoutUrlAsync(cancellationToken);
            return Ok(materials);
        }

        /// <summary>
        /// Create a new material
        /// </summary>
        /// <param name="dto">Material data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created material</returns>
        [HttpPost]
        [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MaterialDto>> CreateMaterial(
            [FromBody] MaterialDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var material = await _materialService.CreateMaterialAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetMaterialWithLesson), new { id = material.Id }, material);
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing material
        /// </summary>
        /// <param name="id">Material identifier</param>
        /// <param name="dto">Material update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated material</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(
            int id,
            [FromBody] MaterialDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var material = await _materialService.UpdateMaterialAsync(id, dto, cancellationToken);
                return Ok(material);
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
        /// Reorder materials within a lesson
        /// </summary>
        /// <param name="lessonId">Lesson identifier</param>
        /// <param name="orderedMaterialIds">Ordered list of material IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpPatch("lesson/{lessonId:int}/reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReorderMaterials(
            int lessonId,
            [FromBody] List<int> orderedMaterialIds,
            CancellationToken cancellationToken)
        {
            if (orderedMaterialIds == null || !orderedMaterialIds.Any())
                return BadRequest(new { message = "Ordered material IDs cannot be empty." });

            try
            {
                await _materialService.ReorderMaterialsAsync(lessonId, orderedMaterialIds, cancellationToken);
                return NoContent();
            }
            catch (BusinessException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a material
        /// </summary>
        /// <param name="id">Material identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteMaterial(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _materialService.DeleteMaterialAsync(id, cancellationToken);
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