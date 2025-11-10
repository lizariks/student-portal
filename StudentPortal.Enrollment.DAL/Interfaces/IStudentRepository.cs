namespace StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
public interface IStudentRepository:IGenericRepository<Student>
{
    Task<Student?> GetByEmailAsync(string email, CancellationToken ct = default);
}
