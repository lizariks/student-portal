namespace StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
public interface ICourseRepository:IGenericRepository<Course>
{
    Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync(CancellationToken ct = default);
}
