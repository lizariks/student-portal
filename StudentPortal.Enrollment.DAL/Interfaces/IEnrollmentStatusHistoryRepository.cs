using StudentPortal.Enrollment.Domain;
namespace StudentPortal.Enrollment.DAL.Interfaces;

public interface IEnrollmentStatusHistoryRepository:IGenericRepository<EnrollmentStatusHistory>
{
    Task<IEnumerable<EnrollmentStatusHistory>> GetByEnrollmentAsync(int enrollmentId, CancellationToken ct = default); 
}
