namespace StudentPortal.CourseCatalogService.BLL.Consumers.Modules;

using StudentPortal.Shared.Events.Modules;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

    public class ModuleDeletedEventConsumer : IConsumer<ModuleDeletedEvent>
    {
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;
        private readonly ILogger<ModuleDeletedEventConsumer> _logger;

        public ModuleDeletedEventConsumer(
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService,
            ILogger<ModuleDeletedEventConsumer> logger)
        {
            _courseCacheInvalidationService = courseCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ModuleDeletedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogWarning(
                "CCS received ModuleDeletedEvent: ModuleId={ModuleId}. Invalidating parent Course cache.",
                message.ModuleId);

            try
            {
                await _courseCacheInvalidationService.InvalidateByIdAsync(message.CourseId); 

                _logger.LogInformation(
                    "Successfully invalidated Course cache for CourseId={CourseId} after Module deletion.",
                    message.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate Course cache for ModuleDeletedEvent: ModuleId={ModuleId}",
                    message.ModuleId);
                throw;
            }
        }
    }