namespace StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;

    public interface IStudentCourseService
    {
        Task<StudentCourse> EnrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default);
        Task UnenrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StudentCourse>> GetEnrollmentsByUserAsync(int userId);
        Task<IEnumerable<StudentCourse>> GetEnrollmentsByCourseAsync(int courseId);
        Task<int> GetEnrollmentCountForCourseAsync(int courseId);
        Task<bool> IsUserEnrolledAsync(int userId, int courseId);
    }
