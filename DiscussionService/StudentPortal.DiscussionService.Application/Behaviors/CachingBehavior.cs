namespace StudentPortal.DiscussionService.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IMemoryCache _cache;

    public CachingBehavior(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = $"{typeof(TRequest).FullName}-{request.GetHashCode()}";

        if (_cache.TryGetValue(cacheKey, out TResponse response))
        {
            return response;
        }

        response = await next();

        if (request.GetType().Name.EndsWith("Query"))
        {
            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
        }

        return response;
    }
}
