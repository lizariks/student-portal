
using MediatR;

namespace StudentPortal.DiscussionService.Application.Interfaces.Commands;
    public interface ICommand : IRequest<Unit>
    {
    }

    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }

