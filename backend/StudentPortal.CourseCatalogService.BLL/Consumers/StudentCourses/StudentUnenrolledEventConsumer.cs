namespace StudentPortal.CourseCatalogService.BLL.Consumers.StudentCourses;

using StudentPortal.Shared.Events.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

    public class StudentUnenrolledEventConsumer : IConsumer<StudentUnenrolledEvent>
    {
        private readonly IStudentCounterService _studentCounterService;
        private readonly IEntityCacheInvalidationService<Course> _courseCacheInvalidationService;
        private readonly ILogger<StudentUnenrolledEventConsumer> _logger;

        public StudentUnenrolledEventConsumer(
            IStudentCounterService studentCounterService,
            IEntityCacheInvalidationService<Course> courseCacheInvalidationService,
            ILogger<StudentUnenrolledEventConsumer> logger)
        {
            _studentCounterService = studentCounterService;
            _courseCacheInvalidationService = courseCacheInvalidationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StudentUnenrolledEvent> context)
        {
            var message = context.Message;
            
            _logger.LogWarning(
                "CCS received StudentUnenrolledEvent: UserId={UserId}, CourseId={CourseId}. Decrementing student count.",
                message.UserId, message.CourseId);

            try
            {
                
                await _courseCacheInvalidationService.InvalidateByIdAsync(message.CourseId); 

                _logger.LogInformation(
                    "Successfully decremented student count and invalidated cache for CourseId={CourseId}",
                    message.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process StudentUnenrolledEvent for CourseId={CourseId}",
                    message.CourseId);
                throw;
            }
        }
    }