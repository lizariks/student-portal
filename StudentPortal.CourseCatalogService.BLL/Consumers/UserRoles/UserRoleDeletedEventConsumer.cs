namespace StudentPortal.CourseCatalogService.BLL.Consumers.UserRoles;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.Shared.Events.UserRoles;
using MassTransit;
using Microsoft.Extensions.Logging;

    public class UserRoleDeletedEventConsumer : IConsumer<UserRoleDeletedEvent>
    {
        private readonly IEntityCacheInvalidationService<UserRole> _cacheInvalidationService;
        private readonly ILogger<UserRoleDeletedEventConsumer> _logger;

        public UserRoleDeletedEventConsumer(
            IEntityCacheInvalidationService<UserRole> cacheInvalidationService,
            ILogger<UserRoleDeletedEventConsumer> logger)
        {
            _cacheInvalidationService = cacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRoleDeletedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Received UserRoleDeletedEvent: UserId={UserId}, RoleId={RoleId}",
                message.UserId, message.RoleId);

            try
            {
                await _cacheInvalidationService.InvalidateByIdAsync(message.UserId);

                _logger.LogInformation(
                    "Successfully invalidated cache after UserRole deletion: UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate cache for UserRoleDeletedEvent: UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }