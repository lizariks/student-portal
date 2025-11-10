namespace StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
using StudentPortal.Enrollment.BLL.DTOs;
public interface IEnrollmentStatusHistoryService
{
    Task<EnrollmentStatusHistory> AddStatusAsync(EnrollmentStatusHistoryUpdateDto dto);
    Task<IEnumerable<EnrollmentStatusHistory>> GetByEnrollmentAsync(int enrollmentId);
}