namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users;

using StudentPortal.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;

    public interface IDiscussionUserProfileService
    {
        Task CreateProfileAsync(int userId, string nickname);
        Task UpdateProfileDisplayNameAsync(int userId, string nickname, string firstName, string lastName);
    }

    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IDiscussionUserProfileService _userProfileService;
        private readonly ILogger<UserCreatedEventConsumer> _logger;

        public UserCreatedEventConsumer(
            IDiscussionUserProfileService userProfileService,
            ILogger<UserCreatedEventConsumer> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "DiscussionService received UserCreatedEvent: UserId={UserId}. Creating discussion profile.",
                message.UserId);

            await _userProfileService.CreateProfileAsync(message.UserId, message.Nickname);
            
            _logger.LogInformation(
                "Discussion profile created for UserId={UserId}",
                message.UserId);
        }
    }