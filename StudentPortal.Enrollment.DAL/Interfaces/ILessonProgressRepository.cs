namespace StudentPortal.Enrollment.DAL.Interfaces;

using StudentPortal.Enrollment.Domain.Entities;

public interface ILessonProgressRepository : IGenericRepository<LessonProgress>
{
    Task<IEnumerable<LessonProgress>> GetByStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct = default);
    Task<IEnumerable<LessonProgress>> GetByCourseAsync(int courseId, CancellationToken ct = default);
    Task DeleteByStudentAndLessonAsync(int studentId, int lessonId, CancellationToken ct = default);
}
