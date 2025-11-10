namespace StudentPortal.DiscussionService.GrpcServer.Service;

using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StudentPortal.Discussion.Grpc;
using StudentPortal.DiscussionService.Application.Services;
using StudentPortal.DiscussionService.Domain.Exceptions;
using DomainUserInfo = StudentPortal.DiscussionService.Domain.ValueObjects.UserInfo;
using DomainTargetType = StudentPortal.DiscussionService.Domain.Enums.TargetType;
using DomainComment = StudentPortal.DiscussionService.Domain.Entities.Comment;


public class DiscussionGrpcServiceImpl : Discussion.DiscussionBase
{
    private readonly DiscussionThreadService _threadService;
    private readonly IMapper _mapper;
    private readonly ILogger<DiscussionGrpcServiceImpl> _logger;

    public DiscussionGrpcServiceImpl(
        DiscussionThreadService threadService,
        IMapper mapper,
        ILogger<DiscussionGrpcServiceImpl> logger)
    {
        _threadService = threadService;
        _mapper = mapper;
        _logger = logger;
    }

    private RpcException HandleException(Exception ex)
    {
        var statusCode = StatusCode.Internal;

        if (ex is NotFoundException)
            statusCode = StatusCode.NotFound;
        else if (ex is UnauthorizedActionException)
            statusCode = StatusCode.PermissionDenied;
        else if (ex is ThreadClosedException)
            statusCode = StatusCode.FailedPrecondition; 
        else if (ex is InvalidContentException || ex is ArgumentException)
            statusCode = StatusCode.InvalidArgument;

        _logger.LogError(ex, "gRPC Error: {MessageType}", ex.GetType().Name);
        return new RpcException(new Status(statusCode, ex.Message));
    }

    public override async Task<DiscussionThreadResponse> CreateThread(
        CreateThreadRequest request,
        ServerCallContext context)
    {
        try
        {
            var targetType = _mapper.Map<DomainTargetType>(request.TargetType);
            var createdBy = _mapper.Map<DomainUserInfo>(request.CreatedBy);

            var thread = await _threadService.CreateThreadAsync(
                request.TargetId,
                targetType,
                request.Title,
                createdBy,
                context.CancellationToken);

            return new DiscussionThreadResponse
            {
                Thread = _mapper.Map<DiscussionThread>(thread)
            };
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }

    public override async Task<DiscussionThreadResponse> GetThreadById(
        GetThreadByIdRequest request,
        ServerCallContext context)
    {
        try
        {
            var thread = await _threadService.GetThreadByIdAsync(
                request.ThreadId,
                context.CancellationToken);

            if (thread == null)
            {
                throw new NotFoundException($"Thread with Id '{request.ThreadId}' not found.");
            }

            return new DiscussionThreadResponse
            {
                Thread = _mapper.Map<DiscussionThread>(thread)
            };
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }

    public override async Task<DiscussionThreadResponse> AddComment(
        AddCommentRequest request,
        ServerCallContext context)
    {
        try
        {
            
            var comment = _mapper.Map<DomainComment>(request);

            await _threadService.AddCommentAsync(
                request.ThreadId,
                comment,
                context.CancellationToken);

            var updatedThread = await _threadService.GetThreadByIdAsync(request.ThreadId, context.CancellationToken);

            if (updatedThread == null)
            {
                throw new NotFoundException($"Thread with Id '{request.ThreadId}' not found after update.");
            }

            return new DiscussionThreadResponse
            {
                Thread = _mapper.Map<DiscussionThread>(updatedThread)
            };
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }
}