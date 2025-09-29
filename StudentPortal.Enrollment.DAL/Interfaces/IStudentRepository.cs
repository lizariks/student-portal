namespace StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
public interface IStudentRepository:IGenericRepository<Student>
{
    Task<Student?> GetByEmailAsync(string email, CancellationToken ct = default);
}
