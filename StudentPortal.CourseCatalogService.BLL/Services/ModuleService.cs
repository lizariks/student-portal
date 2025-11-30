using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.Shared.Events.Modules; 
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.BLL.Metrics;
using StudentPortal.ServiceDefaults.Metrics; 
using MassTransit; 
using Microsoft.Extensions.Logging;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ModuleService> _logger;
        private readonly IPublishEndpoint _publishEndpoint; 
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Module> _moduleCacheInvalidationService;

        public ModuleService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,  
            ILogger<ModuleService> logger,
            IPublishEndpoint publishEndpoint, 
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService,
            IEntityCacheInvalidationService<Module> moduleCacheInvalidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _courseCacheInvalidationService = courseCacheInvalidationService;
            _moduleCacheInvalidationService = moduleCacheInvalidationService;
        }
        public async Task<PagedList<ModuleDto>> GetPagedModulesAsync(ModuleParameters parameters,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.List,
                async () =>
                {
                    var pagedModules = await _unitOfWork.Modules.GetPagedModulesAsync(parameters, cancellationToken);
                    var mappedItems = _mapper.Map<IEnumerable<ModuleDto>>(pagedModules);

                    return new PagedList<ModuleDto>(
                        mappedItems.ToList(),
                        pagedModules.TotalCount,
                        pagedModules.Page,
                        pagedModules.PageSize
                    );
                });
        }
        
        public async Task<ModuleDto?> GetModuleWithLessonsAsync(int moduleId,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.GetById,
                async () =>
                {
                    var module = await _unitOfWork.Modules.GetModuleWithLessonsAsync(moduleId, cancellationToken);
                    
                    
                    return module == null ? null : _mapper.Map<ModuleDto>(module);
                });
        }
        
        public async Task<IEnumerable<ModuleDto>> GetModulesByCourseAsync(int courseId,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "list_modules_by_course",       
                async () =>
                {
                    var modules = await _unitOfWork.Modules.GetModulesByCourseAsync(courseId, cancellationToken);
                    return _mapper.Map<IEnumerable<ModuleDto>>(modules);
                });
        }

        public async Task<ModuleDto> CreateModuleAsync(ModuleDto moduleDto,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Create,
                async () =>
                {
                    if (moduleDto == null)
                        throw new BusinessException("Module cannot be null.");

                    var course = await _unitOfWork.Courses.GetByIdAsync(moduleDto.CourseId, true, cancellationToken);
                    if (course == null)
                        throw new NotFoundException($"Course with id {moduleDto.CourseId} not found.");

                    var module = _mapper.Map<Module>(moduleDto);
                    await _unitOfWork.Modules.AddAsync(module, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var @event = new ModuleCreatedEvent
                    {
                        ModuleId = module.Id,
                        CourseId = module.CourseId,
                        Title = module.Title,
                        Order = module.Order
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogInformation("Published ModuleCreatedEvent for Module {ModuleId}", module.Id);
                    
                    await _courseCacheInvalidationService.InvalidateByIdAsync(module.CourseId);
                    
                    CourseMetrics.CoursesCreated.Add(1, MetricConstants.Tags.OperationCreate); 

                    return _mapper.Map<ModuleDto>(module);
                });
        }

        public async Task<ModuleDto> UpdateModuleAsync(ModuleDto moduleDto,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Update,
                async () =>
                {
                    var existing = await _unitOfWork.Modules.GetByIdAsync(moduleDto.Id, false, cancellationToken);
                    if (existing == null)
                        throw new NotFoundException($"Module with id {moduleDto.Id} not found.");

                    _mapper.Map(moduleDto, existing);

                    await _unitOfWork.Modules.UpdateAsync(existing);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _moduleCacheInvalidationService.InvalidateByIdAsync(moduleDto.Id);
                    await _courseCacheInvalidationService.InvalidateByIdAsync(existing.CourseId);
                    CourseMetrics.CoursesUpdated.Add(1, MetricConstants.Tags.OperationUpdate);


                    return _mapper.Map<ModuleDto>(existing);
                });
        }

        public async Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Delete,
                async () =>
                {
                    var module = await _unitOfWork.Modules.GetByIdAsync(moduleId, false, cancellationToken);
                    if (module == null)
                        throw new NotFoundException($"Module with id {moduleId} not found.");

                    var courseId = module.CourseId;
                    
                    await _unitOfWork.Modules.DeleteAsync(moduleId, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var @event = new ModuleDeletedEvent
                    {
                        ModuleId = moduleId,
                        CourseId = courseId
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogWarning("Published ModuleDeletedEvent for Module {ModuleId}", moduleId);
                    
                    await _moduleCacheInvalidationService.InvalidateByIdAsync(moduleId);
                    await _courseCacheInvalidationService.InvalidateByIdAsync(courseId);
                    
                    CourseMetrics.CoursesDeleted.Add(1, MetricConstants.Tags.OperationDelete);
                });
        }
    }
}
