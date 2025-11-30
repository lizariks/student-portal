using StudentPortal.Shared.Events.Roles;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace StudentPortal.CourseCatalogService.BLL.Consumers.Roles
{
    public class RoleCreatedEventConsumer : IConsumer<RoleCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<Role> _roleCacheInvalidationService;
        private readonly ILogger<RoleCreatedEventConsumer> _logger;

        public RoleCreatedEventConsumer(
            IEntityCacheInvalidationService<Role> roleCacheInvalidationService,
            ILogger<RoleCreatedEventConsumer> logger)
        {
            _roleCacheInvalidationService = roleCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RoleCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "CCS received own event RoleCreatedEvent: RoleId={RoleId}. Invalidating all role list caches.",
                message.RoleId);

            try
            {
                await _roleCacheInvalidationService.InvalidateAllAsync(); 
                
                _logger.LogInformation(
                    "Successfully invalidated all Role caches after creation: RoleId={RoleId}",
                    message.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate Role caches for RoleCreatedEvent: RoleId={RoleId}",
                    message.RoleId);
                throw;
            }
        }
    }
}