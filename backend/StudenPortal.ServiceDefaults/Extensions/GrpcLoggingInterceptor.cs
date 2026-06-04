namespace StudentPortal.ServiceDefaults.Extensions;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using GrpcStatusCode = Grpc.Core.StatusCode;


    public class GrpcLoggingInterceptor : Interceptor
    {
        private readonly ILogger<GrpcLoggingInterceptor> _logger;

        public GrpcLoggingInterceptor(ILogger<GrpcLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var stopwatch = Stopwatch.StartNew();
            var methodName = context.Method;
            var peerAddress = context.Peer;

            _logger.LogInformation(
                "gRPC request started: {GrpcMethod} from {PeerAddress}",
                methodName,
                peerAddress);

            try
            {
                var response = await continuation(request, context);
                stopwatch.Stop();

                _logger.LogInformation(
                    "gRPC request completed: {GrpcMethod} with status {StatusCode} in {Duration}ms",
                    methodName,
                    GrpcStatusCode.OK,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (RpcException rpcEx)
            {
                stopwatch.Stop();
                _logger.LogError(
                    rpcEx,
                    "gRPC request failed: {GrpcMethod} with status {StatusCode} in {Duration}ms. Detail: {ErrorDetail}",
                    methodName,
                    rpcEx.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    rpcEx.Status.Detail);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "gRPC request failed with unexpected error: {GrpcMethod} in {Duration}ms",
                    methodName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }