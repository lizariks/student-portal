namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

    // Інвалідтор кешу для сутності Lesson
    public class LessonCacheInvalidationService : IEntityCacheInvalidationService<Lesson>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<LessonCacheInvalidationService> _logger;

        private const string AllPattern = "lesson:*";

        public LessonCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<LessonCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // Припускаємо, що Lesson.Id є int
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string key = $"lesson:{entityId}";
                await _cacheService.RemoveAsync(key);
                await InvalidateAllAsync(); 

                _logger.LogInformation("Invalidated cache for Lesson {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for Lesson {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                _logger.LogInformation("Invalidated all Lesson-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all Lesson caches");
                throw;
            }
        }
    }