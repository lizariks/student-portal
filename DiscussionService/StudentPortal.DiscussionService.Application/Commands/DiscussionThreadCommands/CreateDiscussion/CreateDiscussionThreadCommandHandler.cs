using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;

public class CreateDiscussionThreadCommandHandler : ICommandHandler<CreateDiscussionThreadCommand, DiscussionThread>
{
    private readonly IDiscussionThreadService _service;

    public CreateDiscussionThreadCommandHandler(IDiscussionThreadService service)
    {
        _service = service;
    }

    public async Task<DiscussionThread> Handle(CreateDiscussionThreadCommand command, CancellationToken cancellationToken)
    {
        return await _service.CreateThreadAsync(
            command.TargetId,
            command.TargetType,
            command.Title,
            command.CreatedBy
        );
    }
}
