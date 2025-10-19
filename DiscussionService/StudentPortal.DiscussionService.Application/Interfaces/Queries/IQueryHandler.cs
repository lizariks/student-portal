using MediatR;
namespace StudentPortal.DiscussionService.Application.Interfaces.Queries;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}