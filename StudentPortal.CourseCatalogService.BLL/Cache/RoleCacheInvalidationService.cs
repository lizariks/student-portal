namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;


    public class RoleCacheInvalidationService : IEntityCacheInvalidationService<Role>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<RoleCacheInvalidationService> _logger;

        private const string AllPattern = "role:*"; // Патерн для всіх записів, пов'язаних з Role

        public RoleCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<RoleCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // Припускаємо, що Role.Id є int
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string key = $"role:{entityId}";
                await _cacheService.RemoveAsync(key);
                await InvalidateAllAsync(); // Інвалідуємо списки при зміні одного елемента

                _logger.LogInformation("Invalidated cache for Role {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for Role {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                _logger.LogInformation("Invalidated all Role-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all Role caches");
                throw;
            }
        }
    }