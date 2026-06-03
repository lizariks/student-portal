namespace StudentPortal.Enrollment.BLL.Services;

using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.Domain.Entities;

public class LessonProgressService : ILessonProgressService
{
    private readonly IUnitOfWork _uow;

    public LessonProgressService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<LessonProgress>> GetByStudentAndCourseAsync(int studentId, int courseId)
        => await _uow.LessonProgress.GetByStudentAndCourseAsync(studentId, courseId);

    public async Task<IEnumerable<LessonProgress>> GetByCourseAsync(int courseId)
        => await _uow.LessonProgress.GetByCourseAsync(courseId);

    public async Task MarkCompleteAsync(int studentId, int lessonId, int courseId)
    {
        var progress = new LessonProgress
        {
            StudentId = studentId,
            LessonId = lessonId,
            CourseId = courseId,
            CompletedAt = DateTime.UtcNow,
        };
        _uow.BeginTransaction();
        try
        {
            await _uow.LessonProgress.AddAsync(progress);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    public async Task MarkIncompleteAsync(int studentId, int lessonId)
    {
        _uow.BeginTransaction();
        try
        {
            await _uow.LessonProgress.DeleteByStudentAndLessonAsync(studentId, lessonId);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
