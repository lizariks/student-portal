using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;

namespace StudentPortal.CourseCatalogService.DAL.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetCourseWithModulesAndLessonsAsync(int courseId, CancellationToken cancellationToken = default);
        Task<Course?> GetCourseWithDetailsAsync(int courseId);

        Task LoadModulesAsync(Course course);
        Task LoadEnrollmentsAsync(Course course);

        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Course>> GetPublishedCoursesAsync();
        Task<IEnumerable<Course>> GetUnpublishedCoursesAsync();
        Task<IEnumerable<Course>> GetCoursesWithMoreThanNStudentsAsync(int count);

        Task<IEnumerable<Course>> SearchCoursesAsync(string keyword);

        Task<IEnumerable<Course>> GetCoursesByStudentAsync(int userId);
    }
}