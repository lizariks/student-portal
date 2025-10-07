
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.Specifications;
using StudentPortal.CourseCatalogService.DAL.Extensions;

namespace StudentPortal.CourseCatalogService.DAL.Repositories
{
    public class StudentCourseRepository : GenericRepository<StudentCourse>, IStudentCourseRepository
    {
        private readonly CourseCatalogDbContext _context;

        public StudentCourseRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PagedList<StudentCourse>> GetPagedStudentCoursesAsync(StudentCourseParameters parameters, CancellationToken cancellationToken = default)
        {
            var spec = new StudentCoursesWithFiltersSpecification(parameters);
            var query = ApplySpecification(spec);

            return await query.ToPagedListAsync(parameters, cancellationToken);
        }
        public async Task<IEnumerable<StudentCourse>> GetEnrollmentsByUserAsync(int userId)
        {
            return await _context.StudentCourses
                .Where(sc => sc.UserId == userId)
                .Include(sc => sc.Course)
                .ThenInclude(c => c.Modules)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentCourse>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Include(sc => sc.User)
                .ToListAsync();
        }

        public async Task<StudentCourse?> GetEnrollmentAsync(int userId, int courseId)
        {
            return await _context.StudentCourses
                .Include(sc => sc.Course)
                .Include(sc => sc.User)
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.CourseId == courseId);
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await _context.StudentCourses
                .AnyAsync(sc => sc.UserId == userId && sc.CourseId == courseId);
        }

        public async Task<int> GetEnrollmentCountForCourseAsync(int courseId)
        {
            return await _context.StudentCourses
                .CountAsync(sc => sc.CourseId == courseId);
        }

        public async Task<IEnumerable<StudentCourse>> GetRecentEnrollmentsAsync(int count)
        {
            return await _context.StudentCourses
                .OrderByDescending(sc => sc.EnrolledAt)
                .Take(count)
                .Include(sc => sc.User)
                .Include(sc => sc.Course)
                .ToListAsync();
        }

        public async Task LoadCourseAsync(StudentCourse enrollment)
        {
            if (enrollment == null) return;
            await _context.Entry(enrollment)
                .Reference(sc => sc.Course)
                .LoadAsync();
        }

        public async Task LoadUserAsync(StudentCourse enrollment)
        {
            if (enrollment == null) return;
            await _context.Entry(enrollment)
                .Reference(sc => sc.User)
                .LoadAsync();
        }
    }
}
