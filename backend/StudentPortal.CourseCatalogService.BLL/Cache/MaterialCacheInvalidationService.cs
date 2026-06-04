namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;


    // Інвалідтор кешу для сутності Material
    public class MaterialCacheInvalidationService : IEntityCacheInvalidationService<Material>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<MaterialCacheInvalidationService> _logger;

        private const string AllPattern = "material:*";

        public MaterialCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<MaterialCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // Припускаємо, що Material.Id є int
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string key = $"material:{entityId}";
                await _cacheService.RemoveAsync(key);
                await InvalidateAllAsync(); 

                _logger.LogInformation("Invalidated cache for Material {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for Material {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                _logger.LogInformation("Invalidated all Material-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all Material caches");
                throw;
            }
        }
    }