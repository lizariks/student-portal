using StudentPortal.Shared.Events.Users;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IEntityCacheInvalidationService<User> _userCacheInvalidationService;
        private readonly ILogger<UserCreatedEventConsumer> _logger;

        public UserCreatedEventConsumer(
            IEntityCacheInvalidationService<User> userCacheInvalidationService,
            ILogger<UserCreatedEventConsumer> logger)
        {
            _userCacheInvalidationService = userCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Received UserCreatedEvent: UserId={UserId}. Invalidating user list caches.",
                message.UserId);

            try
            {
                await _userCacheInvalidationService.InvalidateAllAsync(); 
                
                _logger.LogInformation(
                    "Successfully invalidated all User caches after creation: UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate User caches for UserCreatedEvent: UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }
}