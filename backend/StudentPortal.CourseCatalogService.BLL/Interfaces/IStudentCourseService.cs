namespace StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

    public interface IStudentCourseService
    {
        Task<PagedList<StudentCourseDto>> GetPagedStudentCoursesAsync(
            StudentCourseParameters parameters,
            CancellationToken cancellationToken = default);
        Task<StudentCourseDto> EnrollStudentAsync(
            StudentCourseCreateDto dto,
            CancellationToken cancellationToken = default);
        Task UnenrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByUserAsync(int userId);
        Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByCourseAsync(int courseId);
        Task<int> GetEnrollmentCountForCourseAsync(int courseId);
        Task<bool> IsUserEnrolledAsync(int userId, int courseId);
    }
