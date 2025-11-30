using StudentPortal.Shared.Events.Users;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users
{
    public class UserUpdatedEventConsumer : IConsumer<UserUpdatedEvent>
    {
        private readonly IEntityCacheInvalidationService<User> _userCacheInvalidationService;
        private readonly ILogger<UserUpdatedEventConsumer> _logger;

        public UserUpdatedEventConsumer(
            IEntityCacheInvalidationService<User> userCacheInvalidationService,
            ILogger<UserUpdatedEventConsumer> logger)
        {
            _userCacheInvalidationService = userCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Received UserUpdatedEvent: UserId={UserId}. Invalidating specific user cache.",
                message.UserId);

            try
            {
                // При оновленні інвалідуємо кеш конкретного користувача та всі списки
                await _userCacheInvalidationService.InvalidateByIdAsync(message.UserId);
                await _userCacheInvalidationService.InvalidateAllAsync(); 
                
                _logger.LogInformation(
                    "Successfully invalidated User caches after update: UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to invalidate User caches for UserUpdatedEvent: UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }
}