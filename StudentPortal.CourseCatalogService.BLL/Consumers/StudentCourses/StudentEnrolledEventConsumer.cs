namespace StudentPortal.CourseCatalogService.BLL.Consumers.StudentCourses;

using StudentPortal.Shared.Events.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

    public interface IStudentCounterService 
    {
        Task IncrementStudentCountAsync(int courseId);
    }
    
    public class StudentEnrolledEventConsumer : IConsumer<StudentEnrolledEvent>
    {
        private readonly IStudentCounterService _studentCounterService;
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;
        private readonly ILogger<StudentEnrolledEventConsumer> _logger;

        public StudentEnrolledEventConsumer(
            IStudentCounterService studentCounterService,
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService,
            ILogger<StudentEnrolledEventConsumer> logger)
        {
            _studentCounterService = studentCounterService;
            _courseCacheInvalidationService = courseCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StudentEnrolledEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "CCS received StudentEnrolledEvent: UserId={UserId}, CourseId={CourseId}. Incrementing student count.",
                message.UserId, message.CourseId);

            try
            {
                await _studentCounterService.IncrementStudentCountAsync(message.CourseId);
                await _courseCacheInvalidationService.InvalidateByIdAsync(message.CourseId); 

                _logger.LogInformation(
                    "Successfully incremented student count and invalidated cache for CourseId={CourseId}",
                    message.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process StudentEnrolledEvent for CourseId={CourseId}",
                    message.CourseId);
                throw;
            }
        }
    }