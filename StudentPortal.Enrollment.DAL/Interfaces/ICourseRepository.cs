namespace StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
public interface ICourseRepository:IGenericRepository<Course>
{
    Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync(CancellationToken ct = default);
}
