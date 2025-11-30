namespace StudentPortal.CourseCatalogService.BLL.Consumers.Materials;

using StudentPortal.Shared.Events.Materials;
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.Domain.Entities; 
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


    public class MaterialCreatedEventConsumer : IConsumer<MaterialCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<Lesson> _lessonCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Module> _moduleCacheInvalidationService;
        private readonly ILogger<MaterialCreatedEventConsumer> _logger;

        public MaterialCreatedEventConsumer(
            IEntityCacheInvalidationService<Lesson> lessonCacheInvalidationService,
            IEntityCacheInvalidationService<Module> moduleCacheInvalidationService,
            ILogger<MaterialCreatedEventConsumer> logger)
        {
            _lessonCacheInvalidationService = lessonCacheInvalidationService;
            _moduleCacheInvalidationService = moduleCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<MaterialCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "CCS received MaterialCreatedEvent: MaterialId={MaterialId}. Invalidating parent Lesson/Module caches.",
                message.MaterialId);

            try
            {
                await _lessonCacheInvalidationService.InvalidateByIdAsync(message.LessonId); 
                await _moduleCacheInvalidationService.InvalidateAllAsync(); 

                _logger.LogInformation(
                    "Successfully invalidated caches for LessonId={LessonId} and Module after Material creation.",
                    message.LessonId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate caches for MaterialCreatedEvent: MaterialId={MaterialId}",
                    message.MaterialId);
                throw;
            }
        }
    }