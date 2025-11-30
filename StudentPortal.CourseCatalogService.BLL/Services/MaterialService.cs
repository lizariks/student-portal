
using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;
using StudentPortal.Shared.Events.Materials; 
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.BLL.Metrics; 
using StudentPortal.ServiceDefaults.Metrics; 
using MassTransit; 
using Microsoft.Extensions.Logging;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
   public class MaterialService : IMaterialService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MaterialService> _logger;
        private readonly IPublishEndpoint _publishEndpoint; 
        private readonly IEntityCacheInvalidationService<Lesson> _lessonCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Material> _materialCacheInvalidationService;

        public MaterialService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,  
            ILogger<MaterialService> logger,
            IPublishEndpoint publishEndpoint, // 🔔 DI
            IEntityCacheInvalidationService<Lesson> lessonCacheInvalidationService,
            IEntityCacheInvalidationService<Material> materialCacheInvalidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _lessonCacheInvalidationService = lessonCacheInvalidationService;
            _materialCacheInvalidationService = materialCacheInvalidationService;
        }

        public async Task<PagedList<MaterialDto>> GetPagedMaterialsAsync(MaterialParameters parameters, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.List,
                async () =>
                {
                    var pagedMaterials = await _unitOfWork.Materials.GetPagedAsync(parameters, cancellationToken);
                    var mappedItems = _mapper.Map<IEnumerable<MaterialDto>>(pagedMaterials);

                    return new PagedList<MaterialDto>(
                        mappedItems.ToList(),
                        pagedMaterials.TotalCount,
                        pagedMaterials.Page,
                        pagedMaterials.PageSize);
                });
        }

        public async Task<MaterialDto> CreateMaterialAsync(MaterialDto materialDto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Create,
                async () =>
                {
                    if (materialDto == null)
                        throw new BusinessException("Material cannot be null.");
                    var lesson = await _unitOfWork.Lessons.GetLessonWithDetailsAsync(materialDto.LessonId);
                    if (lesson == null)
                        throw new NotFoundException($"Lesson with Id {materialDto.LessonId} not found.");

                    var material = _mapper.Map<Material>(materialDto);
                    await _unitOfWork.Materials.AddAsync(material, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var createdDto = _mapper.Map<MaterialDto>(material);
                    var @event = new MaterialCreatedEvent
                    {
                        MaterialId = material.Id,
                        LessonId = material.LessonId,
                        CourseId = lesson.Module.CourseId,
                        Title = material.Title,
                        Type = material.Type.ToString(),
                        Order = material.Order
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogInformation("Published MaterialCreatedEvent for Material {MaterialId}", material.Id);
                    await _lessonCacheInvalidationService.InvalidateByIdAsync(material.LessonId);
                    CourseMetrics.CoursesCreated.Add(1, MetricConstants.Tags.OperationCreate); 

                    return createdDto;
                });
        }

        public async Task<MaterialDto> UpdateMaterialAsync(int id, MaterialDto materialDto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Update,
                async () =>
                {
                    var existing = await _unitOfWork.Materials.GetByIdAsync(id, false, cancellationToken);
                    if (existing == null)
                        throw new NotFoundException($"Material with id {id} not found.");

                    _mapper.Map(materialDto, existing);
                    
                    await _unitOfWork.Materials.UpdateAsync(existing);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _materialCacheInvalidationService.InvalidateByIdAsync(id);
                    CourseMetrics.CoursesUpdated.Add(1, MetricConstants.Tags.OperationUpdate);

                    return _mapper.Map<MaterialDto>(existing);
                });
        }

        public async Task DeleteMaterialAsync(int id, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Delete,
                async () =>
                {
                    var material = await _unitOfWork.Materials.GetMaterialWithLessonAsync(id);
                    if (material == null)
                        throw new NotFoundException($"Material with id {id} not found.");
                    var lessonId = material.LessonId;
                    var courseId = material.Lesson?.Module?.CourseId ?? 0;
                    
                    await _unitOfWork.Materials.DeleteAsync(id, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    var @event = new MaterialDeletedEvent
                    {
                        MaterialId = material.Id,
                        LessonId = lessonId,
                        CourseId = courseId
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogWarning("Published MaterialDeletedEvent for Material {MaterialId}", material.Id);
                    await _lessonCacheInvalidationService.InvalidateByIdAsync(lessonId);
                    CourseMetrics.CoursesDeleted.Add(1, MetricConstants.Tags.OperationDelete);
                });
        }
        
        
        public async Task ReorderMaterialsAsync(int lessonId, List<int> orderedMaterialIds, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "reorder_material", 
                async () =>
                {
                    var materials = await _unitOfWork.Materials.GetMaterialsByLessonAsync(lessonId);
                    if (!orderedMaterialIds.All(id => materials.Any(m => m.Id == id)))
                        throw new BusinessException("Ordered IDs do not match the materials in the lesson.");

                    for (int i = 0; i < orderedMaterialIds.Count; i++)
                    {
                        var material = materials.First(m => m.Id == orderedMaterialIds[i]);
                        material.Order = i + 1;
                    }

                    foreach (var material in materials)
                        await _unitOfWork.Materials.UpdateAsync(material);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _lessonCacheInvalidationService.InvalidateByIdAsync(lessonId);
                });
        }

        public async Task<MaterialDto?> GetMaterialWithLessonAsync(int id, CancellationToken cancellationToken = default)
        {
            var material = await _unitOfWork.Materials.GetMaterialWithLessonAsync(id);
            return material == null ? null : _mapper.Map<MaterialDto>(material);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsByLessonAsync(lessonId);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByTypeAsync(MaterialType type, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsByTypeAsync(type);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsWithoutUrlAsync(CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsWithoutUrlAsync();
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetOrderedMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetOrderedMaterialsByLessonAsync(lessonId);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }
        
    }
}
