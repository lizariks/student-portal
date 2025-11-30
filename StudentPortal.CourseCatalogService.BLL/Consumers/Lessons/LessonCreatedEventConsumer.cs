using StudentPortal.Shared.Events.Lessons;
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using StudentPortal.ServiceDefaults.Background.Interfaces;

namespace StudentPortal.CourseCatalogService.BLL.Consumers.Lessons
{
    public class LessonCreatedEventConsumer : ServiceDefaults.Background.Interfaces.IConsumer<LessonCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<Module> _moduleCacheInvalidationService;
        private readonly ILogger<LessonCreatedEventConsumer> _logger;

        public LessonCreatedEventConsumer(
            IEntityCacheInvalidationService<Module> moduleCacheInvalidationService,
            ILogger<LessonCreatedEventConsumer> logger)
        {
            _moduleCacheInvalidationService = moduleCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(LessonCreatedEvent message, CancellationToken cancellationToken)
        {
            
            _logger.LogInformation(
                "CCS received own event LessonCreatedEvent: LessonId={LessonId}. Invalidating parent Module cache.",
                message.LessonId);

            try {
                await _moduleCacheInvalidationService.InvalidateAllAsync(); 
                
                _logger.LogInformation(
                    "Successfully invalidated Module caches after Lesson creation: ModuleId={ModuleId}",
                    message.ModuleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate Module caches for LessonCreatedEvent: LessonId={LessonId}",
                    message.LessonId);
                throw;
            }
        }
    }
}