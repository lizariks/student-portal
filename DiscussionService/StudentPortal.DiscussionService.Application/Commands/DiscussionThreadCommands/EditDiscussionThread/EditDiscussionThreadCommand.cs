using StudentPortal.DiscussionService.Application.Interfaces.Commands;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;

namespace StudentPortal.DiscussionService.Application.Commands.DiscussionThreadCommands.EditDiscussionThread;
    public class EditDiscussionThreadCommentCommand : ICommand<DiscussionThread>
    {
        public Guid ThreadId { get; init; }
        public Guid CommentId { get; init; }
        public string NewContent { get; init; } = default!;
        public UserInfo Actor { get; init; } = default!;
    }