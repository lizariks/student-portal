namespace StudentPortal.CourseCatalogService.BLL.Consumers.Lessons;

using StudentPortal.Shared.Events.Lessons;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Background.Interfaces; 
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Threading;

public class LessonDeletedEventConsumer : IConsumer<LessonDeletedEvent>
{
    private readonly IEntityCacheInvalidationService<Module> _moduleCacheInvalidationService;
    private readonly ILogger<LessonDeletedEventConsumer> _logger;

    public LessonDeletedEventConsumer(
        IEntityCacheInvalidationService<Module> moduleCacheInvalidationService,
        ILogger<LessonDeletedEventConsumer> logger)
    {
        _moduleCacheInvalidationService = moduleCacheInvalidationService;
        _logger = logger;
    }

    public async Task Consume(LessonDeletedEvent message, CancellationToken cancellationToken)
    {
        
        _logger.LogWarning(
            "CCS received own event LessonDeletedEvent: LessonId={LessonId}. Invalidating parent Module cache.",
            message.LessonId);

        try
        {
            await _moduleCacheInvalidationService.InvalidateAllAsync();

            _logger.LogInformation(
                "Successfully invalidated Module caches after Lesson deletion: ModuleId={ModuleId}",
                message.ModuleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to invalidate Module caches for LessonDeletedEvent: LessonId={LessonId}",
                message.LessonId);
            throw; 
        }
    }
}