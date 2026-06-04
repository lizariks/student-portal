namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;


    // Інвалідтор кешу для сутності Module
    public class ModuleCacheInvalidationService : IEntityCacheInvalidationService<Module>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<ModuleCacheInvalidationService> _logger;

        private const string AllPattern = "module:*";

        public ModuleCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<ModuleCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // Припускаємо, що Module.Id є int
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string key = $"module:{entityId}";
                await _cacheService.RemoveAsync(key);
                await InvalidateAllAsync(); 

                _logger.LogInformation("Invalidated cache for Module {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for Module {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                _logger.LogInformation("Invalidated all Module-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all Module caches");
                throw;
            }
        }
    }