namespace StudentPortal.CourseCatalogService.DAL.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;


    public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
    {
        Task<IEnumerable<StudentCourse>> GetEnrollmentsByUserAsync(int userId);
        Task<IEnumerable<StudentCourse>> GetEnrollmentsByCourseAsync(int courseId);
        Task<StudentCourse?> GetEnrollmentAsync(int userId, int courseId);
        Task<bool> IsUserEnrolledAsync(int userId, int courseId);
        Task<int> GetEnrollmentCountForCourseAsync(int courseId);
        Task<IEnumerable<StudentCourse>> GetRecentEnrollmentsAsync(int count);
        Task LoadCourseAsync(StudentCourse enrollment);
        Task LoadUserAsync(StudentCourse enrollment);
    }
