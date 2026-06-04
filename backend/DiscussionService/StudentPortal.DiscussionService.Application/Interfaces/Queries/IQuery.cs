using MediatR;
namespace StudentPortal.DiscussionService.Application.Interfaces.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse> { }