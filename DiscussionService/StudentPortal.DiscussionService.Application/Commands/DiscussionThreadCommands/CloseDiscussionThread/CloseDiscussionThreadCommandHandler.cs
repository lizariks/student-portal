namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CloseDiscussionThread;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;
using MediatR;

public class CreateDiscussionThreadCommandHandler 
    : IRequestHandler<CreateDiscussionThreadCommand, DiscussionThread>
{
    private readonly IDiscussionThreadService _service;

    public CreateDiscussionThreadCommandHandler(IDiscussionThreadService service)
    {
        _service = service;
    }

    public async Task<DiscussionThread> Handle(CreateDiscussionThreadCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateThreadAsync(request.TargetId, request.TargetType, request.Title, request.CreatedBy);
    }
}
