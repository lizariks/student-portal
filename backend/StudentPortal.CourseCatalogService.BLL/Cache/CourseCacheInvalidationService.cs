namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid; 
using Microsoft.Extensions.Logging;

    public class CourseCacheInvalidationService : IEntityCacheInvalidationService<Course>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<CourseCacheInvalidationService> _logger;

        private const string CacheKeyPrefix = "course:";
        private const string ListPattern = "courses:page:*";
        private const string AllPattern = "course:*";

        public CourseCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<CourseCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvalidateByIdAsync(int entityId) 
        {
            try
            {
                string key = $"{CacheKeyPrefix}{entityId}";
                await _cacheService.RemoveAsync(key);
                await _cacheService.RemoveByPatternAsync(ListPattern);

                _logger.LogInformation("Invalidated cache for Course {EntityId} and list cache", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for Course {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                await _cacheService.RemoveByPatternAsync(ListPattern);
                _logger.LogInformation("Invalidated all Course-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all Course caches");
                throw;
            }
        }
    }