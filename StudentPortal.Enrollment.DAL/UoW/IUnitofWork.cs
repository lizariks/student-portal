using System;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.Enrollment.DAL.Interfaces;

namespace StudentPortal.Enrollment.DAL.UoW
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IStudentRepository Students { get; }
        IEnrollmentRepository Enrollments { get; }
        IEnrollmentStatusHistoryRepository EnrollmentStatusHistories { get; }
        ICourseRepository Courses { get; }
        void BeginTransaction();
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}