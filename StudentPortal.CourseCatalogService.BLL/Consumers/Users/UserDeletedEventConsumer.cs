namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users;

using StudentPortal.Shared.Events.Users;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
    public interface IRelatedDataCleanupService 
    {
        Task CleanupUserRolesAsync(int userId);
        Task CleanupEnrollmentsAsync(int userId);
    }
    
    public class UserDeletedEventConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly IRelatedDataCleanupService _cleanupService;
        private readonly IEntityCacheInvalidationService<UserRole> _userRoleCacheInvalidationService;
        private readonly IEntityCacheInvalidationService<StudentCourse> _enrollmentCacheInvalidationService;
        private readonly ILogger<UserDeletedEventConsumer> _logger;

        public UserDeletedEventConsumer(
            IRelatedDataCleanupService cleanupService,
            IEntityCacheInvalidationService<UserRole> userRoleCacheInvalidationService,
            IEntityCacheInvalidationService<StudentCourse> enrollmentCacheInvalidationService,
            ILogger<UserDeletedEventConsumer> logger)
        {
            _cleanupService = cleanupService;
            _userRoleCacheInvalidationService = userRoleCacheInvalidationService;
            _enrollmentCacheInvalidationService = enrollmentCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogWarning(
                "CCS received UserDeletedEvent: UserId={UserId}. Initiating cleanup of UserRole and StudentCourse data.",
                message.UserId);

            try
            {
                await _cleanupService.CleanupUserRolesAsync(message.UserId); 
                await _cleanupService.CleanupEnrollmentsAsync(message.UserId);

                await _userRoleCacheInvalidationService.InvalidateAllAsync();
                await _enrollmentCacheInvalidationService.InvalidateAllAsync(); 

                _logger.LogInformation(
                    "Successfully cleaned up related data and invalidated caches for UserId={UserId}",
                    message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "CRITICAL FAILURE during UserDeletedEvent cleanup for UserId={UserId}",
                    message.UserId);
                throw;
            }
        }
    }