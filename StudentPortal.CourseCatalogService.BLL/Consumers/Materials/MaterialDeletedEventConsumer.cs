
    using StudentPortal.Shared.Events.Materials;
    using StudentPortal.CourseCatalogService.BLL.Cache;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.ServiceDefaults.Background.Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System;
    using System.Threading;

    namespace StudentPortal.CourseCatalogService.BLL.Consumers.Materials;
    public class MaterialDeletedEventConsumer : IConsumer<MaterialDeletedEvent>
    {
        private readonly IEntityCacheInvalidationService<Lesson> _lessonCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Module> _moduleCacheInvalidationService;
        private readonly ILogger<MaterialDeletedEventConsumer> _logger;

        public MaterialDeletedEventConsumer(
            IEntityCacheInvalidationService<Lesson> lessonCacheInvalidationService,
            IEntityCacheInvalidationService<Module> moduleCacheInvalidationService,
            ILogger<MaterialDeletedEventConsumer> logger)
        {
            _lessonCacheInvalidationService = lessonCacheInvalidationService;
            _moduleCacheInvalidationService = moduleCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(MaterialDeletedEvent message, CancellationToken cancellationToken)
        {
            
            _logger.LogWarning(
                "CCS received MaterialDeletedEvent: MaterialId={MaterialId}. Invalidating parent Lesson/Module caches.",
                message.MaterialId);

            try
            {
                await _lessonCacheInvalidationService.InvalidateByIdAsync(message.LessonId); 
                await _moduleCacheInvalidationService.InvalidateAllAsync(); 

                _logger.LogInformation(
                    "Successfully invalidated caches for LessonId={LessonId} and Module after Material deletion.",
                    message.LessonId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate caches for MaterialDeletedEvent: MaterialId={MaterialId}",
                    message.MaterialId);
                throw;
            }
        }
    }