
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.Enums;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Application.Interfaces.Commands;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.CreateDiscussion;

public class CreateDiscussionThreadCommand : ICommand<DiscussionThread>
{
    public Guid TargetId { get; }
    public TargetType TargetType { get; }
    public string Title { get; }
    public UserInfo CreatedBy { get; }

    public CreateDiscussionThreadCommand(Guid targetId, TargetType targetType, string title, UserInfo createdBy)
    {
        TargetId = targetId;
        TargetType = targetType;
        Title = title;
        CreatedBy = createdBy;
    }
}
