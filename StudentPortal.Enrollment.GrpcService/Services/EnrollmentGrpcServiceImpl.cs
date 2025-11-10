using AutoMapper;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.EnrollmentService.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StudentPortal.Enrollment.BLL.DTOs; 

namespace StudentPortal.Enrollment.GrpcService.Services
{
    public class EnrollmentGrpcServiceImpl : EnrollmentGrpcService.EnrollmentGrpcServiceBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentGrpcServiceImpl> _logger;

        public EnrollmentGrpcServiceImpl(
            IEnrollmentService enrollmentService,
            IMapper mapper,
            ILogger<EnrollmentGrpcServiceImpl> logger)
        {
            _enrollmentService = enrollmentService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<EnrollmentResponse> EnrollStudent(
            EnrollStudentRequest request,
            ServerCallContext context)
        {
            try
            {
                var enrollmentDto = _mapper.Map<EnrollmentDto>(request);

                var createdEnrollment = await _enrollmentService.EnrollStudentAsync(
                    enrollmentDto.StudentId,
                    enrollmentDto.CourseId);

                var response = new EnrollmentResponse
                {
                    Enrollment = _mapper.Map<EnrollmentService.Grpc.Enrollment>(createdEnrollment)
                };

                _logger.LogInformation("Student {StudentId} enrolled on course {CourseId}. Enrollment ID: {EnrollmentId}", 
                    request.StudentId, request.CourseId, response.Enrollment.EnrollmentId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnrollStudent gRPC call for StudentId: {StudentId}, CourseId: {CourseId}", 
                    request.StudentId, request.CourseId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<EnrollmentResponse> GetEnrollmentById(
            GetEnrollmentByIdRequest request,
            ServerCallContext context)
        {
            try
            {
                var enrollmentDomain = await _enrollmentService.GetByIdAsync(request.EnrollmentId);

                if (enrollmentDomain == null)
                {
                    _logger.LogWarning("Enrollment with ID {EnrollmentId} not found.", request.EnrollmentId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Enrollment with ID {request.EnrollmentId} not found."));
                }

                var response = new EnrollmentResponse
                {
                    Enrollment = _mapper.Map<EnrollmentService.Grpc.Enrollment>(enrollmentDomain)
                };

                _logger.LogInformation("Returned enrollment {EnrollmentId} via gRPC", request.EnrollmentId);
                return response;
            }
            catch (RpcException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetEnrollmentById gRPC call for EnrollmentId: {EnrollmentId}", request.EnrollmentId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
        
        public override async Task<EnrollmentResponse> UpdateStatus(
            UpdateEnrollmentStatusRequest request,
            ServerCallContext context)
        {
            try
            {
                await _enrollmentService.UpdateStatusAsync(request.EnrollmentId, request.NewStatus);

                var updatedEnrollment = await _enrollmentService.GetByIdAsync(request.EnrollmentId);
                
                if (updatedEnrollment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Enrollment with ID {request.EnrollmentId} disappeared after update."));
                }

                var response = new EnrollmentResponse
                {
                    Enrollment = _mapper.Map<EnrollmentService.Grpc.Enrollment>(updatedEnrollment)
                };

                _logger.LogInformation("Updated status for enrollment {EnrollmentId} to {NewStatus}", request.EnrollmentId, request.NewStatus);
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateStatus gRPC call for EnrollmentId: {EnrollmentId}", request.EnrollmentId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
        
        public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteEnrollment(
            GetEnrollmentByIdRequest request,
            ServerCallContext context)
        {
            try
            {
                await _enrollmentService.DeleteAsync(request.EnrollmentId);

                _logger.LogInformation("Deleted enrollment {EnrollmentId} via gRPC", request.EnrollmentId);
                
                return new Google.Protobuf.WellKnownTypes.Empty();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteEnrollment gRPC call for EnrollmentId: {EnrollmentId}", request.EnrollmentId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}