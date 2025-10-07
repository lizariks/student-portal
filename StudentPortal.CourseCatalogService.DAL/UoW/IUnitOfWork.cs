namespace StudentPortal.CourseCatalogService.DAL.UoW;

using System;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Repositories;
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        ILessonRepository Lessons { get; }
        IMaterialRepository Materials { get; }
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IStudentCourseRepository StudentCourses { get; }
        IModuleRepository Modules { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

