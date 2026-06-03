namespace StudentPortal.Enrollment.BLL.Interfaces;

using StudentPortal.Enrollment.Domain.Entities;

public interface ILessonProgressService
{
    Task<IEnumerable<LessonProgress>> GetByStudentAndCourseAsync(int studentId, int courseId);
    Task<IEnumerable<LessonProgress>> GetByCourseAsync(int courseId);
    Task MarkCompleteAsync(int studentId, int lessonId, int courseId);
    Task MarkIncompleteAsync(int studentId, int lessonId);
}
