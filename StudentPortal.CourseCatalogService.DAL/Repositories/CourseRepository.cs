using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.DAL.Data;

namespace StudentPortal.CourseCatalogService.DAL.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly CourseCatalogDbContext _context;

        public CourseRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Course?> GetCourseWithModulesAndLessonsAsync(int courseId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
        }//eager loading
        
        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task LoadModulesAsync(Course course)
        {
            if (course == null) return;
            await _context.Entry(course)
                .Collection(c => c.Modules)
                .LoadAsync();
        }

        
        public async Task LoadEnrollmentsAsync(Course course)
        {
            if (course == null) return;
            await _context.Entry(course)
                .Collection(c => c.Enrollments)
                .Query()
                .Include(e => e.User)
                .LoadAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetPublishedCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetUnpublishedCoursesAsync()
        {
            return await _context.Courses
                .Where(c => !c.IsPublished)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesWithMoreThanNStudentsAsync(int count)
        {
            return await _context.Courses
                .Where(c => c.Enrollments.Count() > count)
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Enumerable.Empty<Course>();

            keyword = keyword.ToLower();

            return await _context.Courses
                .Where(c => c.Code.ToLower().Contains(keyword) ||
                            c.Title.ToLower().Contains(keyword) ||
                            (c.Description != null && c.Description.ToLower().Contains(keyword)))
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(int userId)
        {
            return await _context.Courses
                .Where(c => c.Enrollments.Any(e => e.UserId == userId))
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .ToListAsync();
        }
    }
}
