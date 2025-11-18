using StudentPortal.AggregatorService.DTOs.Discussion;
using StudentPortal.Discussion.Grpc;
using Grpc.Core;

namespace StudentPortal.AggregatorService.Clients;

public class DiscussionGrpcClient
{
    private readonly  StudentPortal.Discussion.Grpc.Discussion.DiscussionClient _client;
    private readonly ILogger<DiscussionGrpcClient> _logger;

    public DiscussionGrpcClient(
        StudentPortal.Discussion.Grpc.Discussion.DiscussionClient client,
        ILogger<DiscussionGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<DiscussionThreadDto>?> GetAllDiscussionsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Requesting all discussion threads via gRPC.");
            
            return new List<DiscussionThreadDto>(); 
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching all discussions.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all discussions.");
            return null;
        }
    }

    public async Task<DiscussionThreadDto?> GetDiscussionByIdAsync(string discussionId, CancellationToken ct = default)
    {
        try
        {
            var request = new GetThreadByIdRequest { ThreadId = discussionId };
            var response = await _client.GetThreadByIdAsync(request, cancellationToken: ct);
            
            return MapToDto(response.Thread);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Discussion thread {ThreadId} not found.", discussionId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching thread {ThreadId}.", discussionId);
            throw;
        }
    }
    
    public async Task<CourseReviewDto?> GetReviewSummaryByCourseIdAsync(int courseId, CancellationToken ct = default)
    {
        _logger.LogWarning("GetReviewSummaryByCourseIdAsync needs a specific gRPC endpoint in Discussion Service.");
        return null; 
    }

    private static DiscussionThreadDto? MapToDto(Discussion.Grpc.DiscussionThread grpcThread)
    {
        if (grpcThread is null) return null;
        
        return new DiscussionThreadDto
        {
            Id = grpcThread.Id,
            TargetId = grpcThread.TargetId,
            Title = grpcThread.Title,
            IsClosed = grpcThread.IsClosed,
        };
    }
}