using StudentPortal.CourseCatalogService.DAL.Repositories;

namespace StudentPortal.CourseCatalogService.DAL.UoW;

using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using System.Threading;
using System.Threading.Tasks;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly CourseCatalogDbContext _context;

        public ICourseRepository Courses { get; }
        public ILessonRepository Lessons { get; }
        public IMaterialRepository Materials { get; }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IStudentCourseRepository StudentCourses { get; }
        public IModuleRepository Modules { get; }

        public UnitOfWork(CourseCatalogDbContext context)
        {
            _context = context;

            Courses = new CourseRepository(_context);
            Lessons = new LessonRepository(_context);
            Materials = new MaterialRepository(_context);
            Users = new UserRepository(_context);
            Roles = new RoleRepository(_context);
            StudentCourses = new StudentCourseRepository(_context);
            Modules = new ModuleRepository(_context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
