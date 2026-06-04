using StudentPortal.AggregatorService.DTOs.CourseCatalog;
using StudentPortal.CourseCatalog.Grpc;
using Grpc.Core;

namespace StudentPortal.AggregatorService.Clients;

public class CourseCatalogGrpcClient
{
    private readonly StudentPortal.CourseCatalog.Grpc.CourseCatalog.CourseCatalogClient _client;
    
    private readonly ILogger<CourseCatalogGrpcClient> _logger;

    public CourseCatalogGrpcClient(
    StudentPortal.CourseCatalog.Grpc.CourseCatalog.CourseCatalogClient client,
    ILogger<CourseCatalogGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<CourseDto>?> GetAllCoursesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogWarning("GetAllCoursesAsync method not implemented in gRPC yet. Returning mock.");
            return new List<CourseDto> 
            {
                new CourseDto { Id = 999, Title = "Mock Course", Code = "MOCK", Description = "Test" } 
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching all courses from Catalog Service");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all courses from Catalog Service");
            return null;
        }
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int courseId, CancellationToken ct = default)
    {
        try
        {
            var request = new GetCourseByIdRequest { Id = courseId };

            var response = await _client.GetCourseByIdAsync(request, cancellationToken: ct);
            
            return MapToDto(response.Course);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Course {CourseId} not found in Catalog Service.", courseId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching course {CourseId}", courseId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching course {CourseId}", courseId);
            throw;
        }
    }

    private static CourseDto? MapToDto(CourseCatalog.Grpc.Course grpcCourse)
    {
        if (grpcCourse is null) return null;
        
        return new CourseDto
        {
            Id = grpcCourse.Id,
            Code = grpcCourse.Code,
            Title = grpcCourse.Title,
            Description = grpcCourse.Description,
        };
    }
}