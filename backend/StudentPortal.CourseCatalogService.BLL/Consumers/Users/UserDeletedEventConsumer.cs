using StudentPortal.Shared.Events.Users;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users
{
    public class UserDeletedEventConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly IEntityCacheInvalidationService<User> _userCacheInvalidationService;
        private readonly ILogger<UserDeletedEventConsumer> _logger;

        public UserDeletedEventConsumer(
            IEntityCacheInvalidationService<User> userCacheInvalidationService,
            ILogger<UserDeletedEventConsumer> logger)
        {
            _userCacheInvalidationService = userCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Received UserDeletedEvent: UserId={UserId}. Invalidating specific user cache and lists.",
                message.UserId);

            try
            {
                // При видаленні інвалідуємо кеш конкретного користувача та всі списки
                await _userCacheInvalidationService.InvalidateByIdAsync(message.UserId);
                await _userCacheInvalidationService.InvalidateAllAsync(); 
                
                _logger.LogInformation(
                    "Successfully invalidated User caches after deletion: UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate User caches for UserDeletedEvent: UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }
}