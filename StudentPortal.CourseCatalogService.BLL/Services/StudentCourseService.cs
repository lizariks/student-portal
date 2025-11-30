using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.Shared.Events.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Cache; 
using StudentPortal.CourseCatalogService.BLL.Metrics; 
using StudentPortal.ServiceDefaults.Metrics; 
using MassTransit; 
using Microsoft.Extensions.Logging;

namespace StudentPortal.CourseCatalogService.BLL.Services;
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentCourseService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IEntityCacheInvalidationService<StudentCourse> _enrollmentCacheInvalidationService; 
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService; 


        public StudentCourseService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ILogger<StudentCourseService> logger,
            IPublishEndpoint publishEndpoint,
            IEntityCacheInvalidationService<StudentCourse> enrollmentCacheInvalidationService,
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _enrollmentCacheInvalidationService = enrollmentCacheInvalidationService;
            _courseCacheInvalidationService = courseCacheInvalidationService;
        }

        public async Task<StudentCourseDto> EnrollStudentAsync(
            StudentCourseCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "enroll_student",
                async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId, true, cancellationToken);
                    if (user == null)
                        throw new NotFoundException($"User with id {dto.UserId} not found.");

                    var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId, true, cancellationToken);
                    if (course == null)
                        throw new NotFoundException($"Course with id {dto.CourseId} not found.");

                    if (!course.IsPublished)
                        throw new BusinessException("Cannot enroll in an unpublished course.");

                    bool alreadyEnrolled = await _unitOfWork.StudentCourses.IsUserEnrolledAsync(dto.UserId, dto.CourseId);
                    if (alreadyEnrolled)
                        throw new BusinessException("Student is already enrolled in this course.");

                    var enrollment = _mapper.Map<StudentCourse>(dto);
                    enrollment.EnrolledAt = DateTime.UtcNow;

                    await _unitOfWork.StudentCourses.AddAsync(enrollment, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var @event = new StudentEnrolledEvent
                    {
                        UserId = enrollment.UserId,
                        CourseId = enrollment.CourseId,
                        EnrolledAt = enrollment.EnrolledAt
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogInformation("Published StudentEnrolledEvent for UserId {UserId} on CourseId {CourseId}", dto.UserId, dto.CourseId);

                    await _enrollmentCacheInvalidationService.InvalidateAllAsync();
                    await _courseCacheInvalidationService.InvalidateByIdAsync(dto.CourseId);
                    
                    CourseMetrics.CoursesCreated.Add(1, new System.Diagnostics.TagList(MetricConstants.Tags.OperationCreate) { { "entity", "Enrollment" } });

                    return _mapper.Map<StudentCourseDto>(enrollment);
                });
        }


        public async Task UnenrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "unenroll_student",
                async () =>
                {
                    var enrollment = await _unitOfWork.StudentCourses.GetEnrollmentAsync(userId, courseId);
                    if (enrollment == null)
                        throw new NotFoundException("Enrollment not found.");
                    
                    var enrollmentUserId = enrollment.UserId;
                    var enrollmentCourseId = enrollment.CourseId;

                    await _unitOfWork.StudentCourses.DeleteAsync(enrollment.UserId, cancellationToken); 
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var @event = new StudentUnenrolledEvent
                    {
                        UserId = enrollmentUserId,
                        CourseId = enrollmentCourseId,
                        UnenrolledAt = DateTime.UtcNow
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogWarning("Published StudentUnenrolledEvent for UserId {UserId} on CourseId {CourseId}", enrollmentUserId, enrollmentCourseId);

                    await _enrollmentCacheInvalidationService.InvalidateAllAsync();
                    await _courseCacheInvalidationService.InvalidateByIdAsync(enrollmentCourseId);
                    CourseMetrics.CoursesDeleted.Add(1, new System.Diagnostics.TagList(MetricConstants.Tags.OperationDelete) { { "entity", "Enrollment" } });
                });
        }


        public async Task<PagedList<StudentCourseDto>> GetPagedStudentCoursesAsync(
            StudentCourseParameters parameters,
            CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.List,
                async () =>
                {
                    var pagedEnrollments = await _unitOfWork.StudentCourses.GetPagedStudentCoursesAsync(parameters, cancellationToken);
                    var mappedItems = _mapper.Map<IEnumerable<StudentCourseDto>>(pagedEnrollments);

                    return new PagedList<StudentCourseDto>(
                        mappedItems.ToList(),
                        pagedEnrollments.TotalCount,
                        pagedEnrollments.Page,
                        pagedEnrollments.PageSize
                    );
                });
        }

        public async Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByUserAsync(int userId)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "list_by_user",
                async () =>
                {
                    var enrollments = await _unitOfWork.StudentCourses.GetEnrollmentsByUserAsync(userId);
                    return _mapper.Map<IEnumerable<StudentCourseDto>>(enrollments);
                });
        }

        public async Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "list_by_course",
                async () =>
                {
                    var enrollments = await _unitOfWork.StudentCourses.GetEnrollmentsByCourseAsync(courseId);
                    return _mapper.Map<IEnumerable<StudentCourseDto>>(enrollments);
                });
        }

        public async Task<int> GetEnrollmentCountForCourseAsync(int courseId)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "get_enrollment_count",
                async () =>
                {
                    return await _unitOfWork.StudentCourses.GetEnrollmentCountForCourseAsync(courseId);
                });
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                "check_enrolled",
                async () =>
                {
                    return await _unitOfWork.StudentCourses.IsUserEnrolledAsync(userId, courseId);
                });
        }
    }