namespace StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
public interface IEnrollmentRepository:IGenericRepository<Enrollment>
{
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId, CancellationToken ct = default);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId, CancellationToken ct = default);
    
}
