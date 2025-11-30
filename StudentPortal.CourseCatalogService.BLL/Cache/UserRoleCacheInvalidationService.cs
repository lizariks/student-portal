using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.ServiceDefaults.Hybrid;
using Microsoft.Extensions.Logging;

namespace StudentPortal.CourseCatalogService.BLL.Cache
{
    public class UserRoleCacheInvalidationService : IEntityCacheInvalidationService<UserRole>
    {
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<UserRoleCacheInvalidationService> _logger;

        private const string RolesByUserPattern = "userroles:user:*";
        private const string AllPattern = "userrole:*";

        public UserRoleCacheInvalidationService(
            IHybridCacheService cacheService,
            ILogger<UserRoleCacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task InvalidateByIdAsync(int entityId)
        {
            try
            {
                string specificUserRolesPattern = $"userroles:user:{entityId}*";
                await _cacheService.RemoveByPatternAsync(specificUserRolesPattern);
                
                _logger.LogInformation("Invalidated cache for UserRoles related to UserId={EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for UserRole related to UserId={EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(AllPattern);
                await _cacheService.RemoveByPatternAsync(RolesByUserPattern);

                _logger.LogInformation("Invalidated all UserRole-related caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate all UserRole caches");
                throw;
            }
        }
    }
}