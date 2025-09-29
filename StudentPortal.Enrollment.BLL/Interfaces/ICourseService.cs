namespace StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.Domain;
public interface ICourseService
{
    Task<Course> GetByIdAsync(int id);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(int id);
    Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync();
}