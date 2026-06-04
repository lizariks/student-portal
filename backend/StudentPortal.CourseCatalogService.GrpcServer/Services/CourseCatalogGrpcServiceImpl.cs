namespace StudentPortal.CourseCatalogService.GrpcServer.Services;

using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StudentPortal.CourseCatalog.Grpc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Interfaces;

using Google.Protobuf.WellKnownTypes; 


    public class CourseCatalogGrpcServiceImpl : CourseCatalog.CourseCatalogBase
    {
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseCatalogGrpcServiceImpl> _logger;

        public CourseCatalogGrpcServiceImpl(
            ICourseService courseService,
            IMapper mapper,
            ILogger<CourseCatalogGrpcServiceImpl> logger)
        {
            _courseService = courseService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<CourseResponse> CreateCourse(
            CoursePayload request,
            ServerCallContext context)
        {
            try
            {
                var createDto = _mapper.Map<CourseCreateDto>(request);
                
                var courseDto = await _courseService.CreateCourseAsync(createDto, context.CancellationToken);
                
                var response = new CourseResponse
                {
                    Course = _mapper.Map<Course>(courseDto)
                };

                _logger.LogInformation("Course created with ID: {CourseId}", response.Course.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course via gRPC.");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<CourseResponse> GetCourseById(
            GetCourseByIdRequest request,
            ServerCallContext context)
        {
            try
            {
                var courseDto = await _courseService.GetCourseByIdAsync(request.Id, context.CancellationToken);

                if (courseDto == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found.", request.Id);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Course with ID {request.Id} not found."));
                }

                var response = new CourseResponse
                {
                    Course = _mapper.Map<Course>(courseDto) 
                };

                _logger.LogInformation("Returned course {CourseId} via gRPC", request.Id);
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course by ID {CourseId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<CourseResponse> UpdateCourse(
            Course request,
            ServerCallContext context)
        {
            try
            {
                var updateDto = _mapper.Map<CourseUpdateDto>(request);
                
                var updatedCourseDto = await _courseService.UpdateCourseAsync(request.Id, updateDto, context.CancellationToken);
                
                var response = new CourseResponse
                {
                    Course = _mapper.Map<Course>(updatedCourseDto)
                };

                _logger.LogInformation("Course updated with ID: {CourseId}", request.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course via gRPC for ID: {CourseId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
        public override async Task<Empty> DeleteCourse(
            GetCourseByIdRequest request,
            ServerCallContext context)
        {
            try
            {
                await _courseService.DeleteCourseAsync(request.Id, context.CancellationToken);

                _logger.LogInformation("Course deleted with ID: {CourseId}", request.Id);
                
                return new Empty();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course via gRPC for ID: {CourseId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }