namespace StudentPortal.ServiceDefaults.Handlers;

using Microsoft.AspNetCore.Http;

public class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationHeader = "X-Correlation-Id";

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationHeader]?.ToString()
                            ?? Guid.NewGuid().ToString();

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.TryAddWithoutValidation(CorrelationHeader, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}