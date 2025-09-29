namespace StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain;
public interface IEnrollmentService
{
    Task<IEnumerable<Enrollment>> GetAllAsync();
    Task<Enrollment> GetByIdAsync(int id);
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
    Task<Enrollment> EnrollStudentAsync(int studentId, int courseId);
    Task UpdateStatusAsync(int enrollmentId, string status);
    Task DeleteAsync(int id);
}