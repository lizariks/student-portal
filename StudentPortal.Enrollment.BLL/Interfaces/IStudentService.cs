namespace StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;
    public interface IStudentService
    {
        Task<Student> GetByIdAsync(int id);
        Task<Student> GetByEmailAsync(string email);
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(int id);
    }