namespace StudentPortal.CourseCatalogService.BLL.Consumers.Users;

using StudentPortal.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


    public class UserUpdatedEventConsumer : IConsumer<UserUpdatedEvent>
    {
        private readonly IDiscussionUserProfileService _userProfileService;
        private readonly ILogger<UserUpdatedEventConsumer> _logger;

        public UserUpdatedEventConsumer(
            IDiscussionUserProfileService userProfileService,
            ILogger<UserUpdatedEventConsumer> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "DiscussionService received UserUpdatedEvent: UserId={UserId}. Synchronizing display name.",
                message.UserId);

            await _userProfileService.UpdateProfileDisplayNameAsync(
                message.UserId, 
                message.NewNickname, 
                message.NewFirstName, 
                message.NewLastName);
            
            _logger.LogInformation(
                "Discussion profile display name synchronized for UserId={UserId}",
                message.UserId);
        }
    }