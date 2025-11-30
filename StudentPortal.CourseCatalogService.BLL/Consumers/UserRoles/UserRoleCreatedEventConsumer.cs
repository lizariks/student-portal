namespace StudentPortal.CourseCatalogService.BLL.Consumers.UserRoles;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.Shared.Events.UserRoles;
using MassTransit;
using Microsoft.Extensions.Logging;

    public class UserRoleCreatedEventConsumer : IConsumer<UserRoleCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<UserRole> _cacheInvalidationService;
        private readonly ILogger<UserRoleCreatedEventConsumer> _logger;

        public UserRoleCreatedEventConsumer(
            IEntityCacheInvalidationService<UserRole> cacheInvalidationService,
            ILogger<UserRoleCreatedEventConsumer> logger)
        {
            _cacheInvalidationService = cacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRoleCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Received UserRoleCreatedEvent: UserId={UserId}, RoleId={RoleId}",
                message.UserId, message.RoleId);

            try
            {
                await _cacheInvalidationService.InvalidateByIdAsync(message.UserId);

                _logger.LogInformation(
                    "Successfully invalidated cache after UserRole creation: UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate cache for UserRoleCreatedEvent: UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }