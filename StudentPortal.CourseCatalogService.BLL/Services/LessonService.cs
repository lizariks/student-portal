namespace StudentPortal.CourseCatalogService.BLL.Services;

using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using AutoMapper;
using StudentPortal.Shared.Events.Lessons; 
using StudentPortal.CourseCatalogService.BLL.Metrics;
using StudentPortal.ServiceDefaults.Metrics; 
using StudentPortal.CourseCatalogService.BLL.Cache; 
using MassTransit; 

using Microsoft.Extensions.Logging;


public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IEntityCacheInvalidationService<Lesson> _lessonCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;

        public LessonService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LessonService> logger,
            IPublishEndpoint publishEndpoint,
            IEntityCacheInvalidationService<Lesson> lessonCacheInvalidationService,
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _lessonCacheInvalidationService = lessonCacheInvalidationService;
            _courseCacheInvalidationService = courseCacheInvalidationService;
        }
        public async Task<PagedList<LessonDto>> GetPagedLessonsAsync(
            LessonParameters parameters,
            ISortHelper<Lesson>? sortHelper = null,
            CancellationToken cancellationToken = default)
        {
            var pagedLessons = await _unitOfWork.Lessons.GetPagedLessonsAsync(parameters, sortHelper, cancellationToken);
            var mappedItems = _mapper.Map<IEnumerable<LessonDto>>(pagedLessons);
            return new PagedList<LessonDto>(
                mappedItems.ToList(),
                pagedLessons.TotalCount,
                pagedLessons.Page,
                pagedLessons.PageSize);
        }

        public async Task<LessonDto> CreateLessonAsync(LessonCreateDto dto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Create,
                async () =>
                {
                    var module = await _unitOfWork.Modules.GetByIdAsync(dto.ModuleId, cancellationToken: cancellationToken);
                    if (module == null) throw new NotFoundException($"Module with Id {dto.ModuleId} not found");

                    var lesson = _mapper.Map<Lesson>(dto);
                    await _unitOfWork.Lessons.AddAsync(lesson, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var createdDto = _mapper.Map<LessonDto>(lesson);
                    var @event = new LessonCreatedEvent
                    {
                        LessonId = lesson.Id,
                        ModuleId = lesson.ModuleId,
                        CourseId = module.CourseId, 
                        Title = lesson.Title,
                        EstimatedDuration = lesson.EstimatedDuration,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogInformation("Published LessonCreatedEvent for Lesson {LessonId}", lesson.Id);
                    await _lessonCacheInvalidationService.InvalidateAllAsync();
                    await _courseCacheInvalidationService.InvalidateByIdAsync(module.CourseId);
                    CourseMetrics.CoursesCreated.Add(1, MetricConstants.Tags.OperationCreate);

                    return createdDto;
                });
        }

        public async Task DeleteLessonAsync(int id, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Delete,
                async () =>
                {
                    var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken);
                    if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

                    var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId, cancellationToken: cancellationToken);
                    
                    await _unitOfWork.Lessons.DeleteAsync(lesson.Id, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var @event = new LessonDeletedEvent
                    {
                        LessonId = lesson.Id,
                        ModuleId = lesson.ModuleId,
                        CourseId = module?.CourseId ?? 0,
                        DeletedAt = DateTime.UtcNow 
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogWarning("Published LessonDeletedEvent for Lesson {LessonId}", lesson.Id);
                    
                    await _lessonCacheInvalidationService.InvalidateAllAsync();
                    await _courseCacheInvalidationService.InvalidateByIdAsync(module?.CourseId ?? 0);
                    CourseMetrics.CoursesDeleted.Add(1, MetricConstants.Tags.OperationDelete);
                });
        }
        
        public async Task<LessonDto> UpdateLessonAsync(int id, LessonUpdateDto dto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Update,
                async () =>
                {
                    var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken);
                    if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

                    var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId, cancellationToken: cancellationToken);

                    _mapper.Map(dto, lesson);
                    await _unitOfWork.Lessons.UpdateAsync(lesson);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _lessonCacheInvalidationService.InvalidateByIdAsync(id);
                    await _courseCacheInvalidationService.InvalidateByIdAsync(module?.CourseId ?? 0);

                    CourseMetrics.CoursesUpdated.Add(1, MetricConstants.Tags.OperationUpdate);

                    return _mapper.Map<LessonDto>(lesson);
                });
        }
        

        public async Task ReorderLessonAsync(int lessonId, int newOrder, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "reorder_lesson", 
                async () =>
                {
                    var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId, asNoTracking: false, cancellationToken: cancellationToken);
                    if (lesson == null) throw new NotFoundException($"Lesson with Id {lessonId} not found");
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _lessonCacheInvalidationService.InvalidateAllAsync(); 
                });
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync(CancellationToken cancellationToken = default)
        {
            var lessons = await _unitOfWork.Lessons.GetAllAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<LessonDto> GetLessonByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<LessonDetailDto> GetLessonDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetLessonWithDetailsAsync(id);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

            return _mapper.Map<LessonDetailDto>(lesson);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(moduleId, cancellationToken: cancellationToken);
            if (module == null) throw new NotFoundException($"Module with Id {moduleId} not found");

            var lessons = module.Lessons.OrderBy(l => l.Order).ToList();
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }
        
    }
