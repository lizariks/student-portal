namespace StudentPortal.Enrollment.BLL.Services;

using AutoMapper;using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.Domain.Exceptions;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public EnrollmentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _uow.Enrollments.GetAllAsync();
    }

    public async Task<Enrollment> GetByIdAsync(int id)
    {
        var enrollment = await _uow.Enrollments.GetByIdAsync(id)
                         ?? throw new NotFoundException($"Enrollment {id} not found.");
        return enrollment;
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
    {
        return await _uow.Enrollments.GetByStudentAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId)
    {
        return await _uow.Enrollments.GetByCourseAsync(courseId);
    }

    public async Task<Enrollment> EnrollStudentAsync(int studentId, int courseId)
    {
        var student = await _uow.Students.GetByIdAsync(studentId)
                      ?? throw new NotFoundException($"Student {studentId} not found.");
        var course = await _uow.Courses.GetByIdAsync(courseId)
                     ?? throw new NotFoundException($"Course {courseId} not found.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        };

        _uow.BeginTransaction();
        try
        {
            await _uow.Enrollments.AddAsync(enrollment);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }

        return enrollment;
    }

    public async Task UpdateStatusAsync(int enrollmentId, string status)
    {
        var enrollment = await _uow.Enrollments.GetByIdAsync(enrollmentId)
                         ?? throw new NotFoundException($"Enrollment {enrollmentId} not found.");

        _uow.BeginTransaction();
        try
        {
            enrollment.Status = status;
            await _uow.Enrollments.UpdateAsync(enrollment);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        _uow.BeginTransaction();
        try
        {
            await _uow.Enrollments.DeleteAsync(id);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
