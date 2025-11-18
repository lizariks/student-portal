using StudentPortal.AggregatorService.DTOs.Enrollment; 
using StudentPortal.EnrollmentService.Grpc; 
using Grpc.Core;

namespace StudentPortal.AggregatorService.Clients;

public class EnrollmentGrpcClient
{
    private readonly EnrollmentGrpcService.EnrollmentGrpcServiceClient _client;
    private readonly ILogger<EnrollmentGrpcClient> _logger;

    public EnrollmentGrpcClient(
        EnrollmentGrpcService.EnrollmentGrpcServiceClient client,
        ILogger<EnrollmentGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<EnrollmentDto>?> GetAllEnrollmentsAsync(CancellationToken ct = default)
    {
        try
        {
            
            _logger.LogInformation("Requesting all enrollment source data via gRPC.");
            
            return new List<EnrollmentDto>(); 
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching all enrollments.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all enrollments.");
            return null;
        }
    }

    public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(int enrollmentId, CancellationToken ct = default)
    {
        try
        {
            var request = new GetEnrollmentByIdRequest { EnrollmentId = enrollmentId };
            var response = await _client.GetEnrollmentByIdAsync(request, cancellationToken: ct);
            
            return MapToDto(response.Enrollment);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Enrollment {EnrollmentId} not found.", enrollmentId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching enrollment {EnrollmentId}.", enrollmentId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching enrollment {EnrollmentId}", enrollmentId);
            throw;
        }
    }
    private static EnrollmentDto? MapToDto(EnrollmentService.Grpc.Enrollment grpcEnrollment)
    {
        if (grpcEnrollment is null) return null;
        
        return new EnrollmentDto
        {
            EnrollmentId = grpcEnrollment.EnrollmentId,
            Status = grpcEnrollment.Status,
        };
    }
}