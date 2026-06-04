namespace StudentPortal.CourseCatalogService.BLL.Cache;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;


    // Інвалідтор кешу для сутності User
    public class UserCacheInvalidationService : IEntityCacheInvalidationService<User>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<UserCacheInvalidationService> _logger;

        private const string AllPattern = "user:*";

        public UserCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<UserCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // Припускаємо, що User.Id є int
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string key = $"user:{entityId}";
                await _cacheService.RemoveAsync(key);
                await InvalidateAllAsync(); // Інвалідуємо списки при зміні одного елемента

                _logger.LogInformation("Invalidated cache for User {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for User {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                _logger.LogInformation("Invalidated all User-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all User caches");
                throw;
            }
        }
    }