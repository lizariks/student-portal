namespace StudentPortal.CourseCatalogService.BLL.Consumers.Modules;

using StudentPortal.Shared.Events.Modules;
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

    public class ModuleCreatedEventConsumer : IConsumer<ModuleCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;
        private readonly ILogger<ModuleCreatedEventConsumer> _logger;

        public ModuleCreatedEventConsumer(
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService,
            ILogger<ModuleCreatedEventConsumer> logger)
        {
            _courseCacheInvalidationService = courseCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ModuleCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "CCS received ModuleCreatedEvent: ModuleId={ModuleId}. Invalidating parent Course cache.",
                message.ModuleId);

            try
            {
                await _courseCacheInvalidationService.InvalidateByIdAsync(message.CourseId); 

                _logger.LogInformation(
                    "Successfully invalidated Course cache for CourseId={CourseId} after Module creation.",
                    message.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate Course cache for ModuleCreatedEvent: ModuleId={ModuleId}",
                    message.ModuleId);
                throw;
            }
        }
    }