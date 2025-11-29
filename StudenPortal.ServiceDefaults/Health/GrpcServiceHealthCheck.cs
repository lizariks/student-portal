namespace StudentPortal.ServiceDefaults.Health;

using Grpc.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;


    public abstract class GrpcServiceHealthCheck<TClient> : IHealthCheck
        where TClient : class
    {
        protected readonly TClient Client;
        protected readonly ILogger<GrpcServiceHealthCheck<TClient>> Logger;
        protected virtual TimeSpan Timeout => TimeSpan.FromSeconds(3);
        protected abstract string ServiceName { get; }

        protected GrpcServiceHealthCheck(
            TClient client,
            ILogger<GrpcServiceHealthCheck<TClient>> logger)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(Timeout);

                var isHealthy = await PerformHealthCheckAsync(cts.Token);

                return isHealthy
                    ? HealthCheckResult.Healthy($"{ServiceName} is responsive")
                    : HealthCheckResult.Unhealthy($"{ServiceName} returned unhealthy status");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                Logger.LogWarning(ex, "{ServiceName} unavailable: {Message}", ServiceName, ex.Message);
                return HealthCheckResult.Unhealthy($"{ServiceName} unavailable", ex);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Logger.LogWarning(ex, "{ServiceName} timeout after {Timeout}s", ServiceName, Timeout.TotalSeconds);
                return HealthCheckResult.Unhealthy($"{ServiceName} timeout", ex);
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogWarning(ex, "{ServiceName} health check cancelled", ServiceName);
                return HealthCheckResult.Unhealthy($"{ServiceName} check cancelled", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{ServiceName} health check failed", ServiceName);
                return HealthCheckResult.Unhealthy($"{ServiceName} check failed", ex);
            }
        }

        protected abstract Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken);
    }