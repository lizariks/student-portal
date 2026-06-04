namespace StudentPortal.CourseCatalogService.BLL.Consumers.Roles;

using StudentPortal.Shared.Events.Roles;
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.Domain.Entities; 
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

    public class RoleDeletedEventConsumer : IConsumer<RoleDeletedEvent>
    {
        private readonly IEntityCacheInvalidationService<UserRole> _userRoleCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<Role> _roleCacheInvalidationService;
        private readonly ILogger<RoleDeletedEventConsumer> _logger;

        public RoleDeletedEventConsumer(
            IEntityCacheInvalidationService<UserRole> userRoleCacheInvalidationService,
            IEntityCacheInvalidationService<Role> roleCacheInvalidationService,
            ILogger<RoleDeletedEventConsumer> logger)
        {
            _userRoleCacheInvalidationService = userRoleCacheInvalidationService;
            _roleCacheInvalidationService = roleCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RoleDeletedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogWarning(
                "CCS received own event RoleDeletedEvent: RoleId={RoleId}. Invalidating UserRole and Role caches.",
                message.RoleId);

            try
            {
                await _userRoleCacheInvalidationService.InvalidateAllAsync(); 
                await _roleCacheInvalidationService.InvalidateAllAsync(); 

                _logger.LogInformation(
                    "Successfully invalidated UserRole and Role caches after deletion: RoleId={RoleId}",
                    message.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate caches for RoleDeletedEvent: RoleId={RoleId}",
                    message.RoleId);
                throw;
            }
        }
    }