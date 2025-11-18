using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

namespace StudentPortal.ServiceDefaults.Extensions;
    public enum ResilienceProfile
    {
        Critical,
        Standard,
        ReadOptimized,
        WriteOptimized
    }

    public static class GrpcResilienceExtensions
    {
        public static IHttpClientBuilder AddGrpcResilienceHandler(
            this IHttpClientBuilder builder,
            int maxRetryAttempts = 3,
            int delaySeconds = 1,
            double failureRatio = 0.5,
            int minimumThroughput = 50,
            int breakDurationSeconds = 30,
            int samplingDurationSeconds = 60,
            int timeoutSeconds = 10)
        {
            builder.AddStandardResilienceHandler(resilience =>
            {
                resilience.Retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = maxRetryAttempts,
                    Delay = TimeSpan.FromSeconds(delaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>()
                        .HandleResult(response =>
                            response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                            response.StatusCode == HttpStatusCode.TooManyRequests ||
                            response.StatusCode == HttpStatusCode.RequestTimeout ||
                            response.StatusCode == HttpStatusCode.InternalServerError ||
                            response.StatusCode == HttpStatusCode.BadGateway ||
                            response.StatusCode == HttpStatusCode.GatewayTimeout)
                };

                resilience.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = failureRatio,
                    MinimumThroughput = minimumThroughput,
                    BreakDuration = TimeSpan.FromSeconds(breakDurationSeconds),
                    SamplingDuration = TimeSpan.FromSeconds(samplingDurationSeconds),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>()
                        .HandleResult(response =>
                            response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                            response.StatusCode == HttpStatusCode.InternalServerError ||
                            response.StatusCode == HttpStatusCode.RequestTimeout)
                };

                resilience.TotalRequestTimeout = new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                };

                resilience.AttemptTimeout = new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds / maxRetryAttempts))
                };
            });

            return builder;
        }

        public static IHttpClientBuilder AddGrpcResilienceHandler(
            this IHttpClientBuilder builder,
            ResilienceProfile profile)
        {
            return profile switch
            {
                ResilienceProfile.Critical => builder.AddGrpcResilienceHandler(
                    maxRetryAttempts: 5,
                    delaySeconds: 1,
                    failureRatio: 0.3,
                    minimumThroughput: 20,
                    breakDurationSeconds: 60,
                    samplingDurationSeconds: 30,
                    timeoutSeconds: 15),

                ResilienceProfile.Standard => builder.AddGrpcResilienceHandler(
                    maxRetryAttempts: 3,
                    delaySeconds: 1,
                    failureRatio: 0.5,
                    minimumThroughput: 50,
                    breakDurationSeconds: 30,
                    samplingDurationSeconds: 60,
                    timeoutSeconds: 10),

                ResilienceProfile.ReadOptimized => builder.AddGrpcResilienceHandler(
                    maxRetryAttempts: 5,
                    delaySeconds: 1,
                    failureRatio: 0.6,
                    minimumThroughput: 30,
                    breakDurationSeconds: 20,
                    samplingDurationSeconds: 45,
                    timeoutSeconds: 8),

                ResilienceProfile.WriteOptimized => builder.AddGrpcResilienceHandler(
                    maxRetryAttempts: 2,
                    delaySeconds: 1,
                    failureRatio: 0.4,
                    minimumThroughput: 30,
                    breakDurationSeconds: 45,
                    samplingDurationSeconds: 60,
                    timeoutSeconds: 15),

                _ => builder.AddGrpcResilienceHandler()
            };
        }
    }