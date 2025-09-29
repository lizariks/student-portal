namespace StudentPortal.Enrollment.BLL.Services;

using AutoMapper;
using StudentPortal.Enrollment.BLL.DTOs;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.Domain.Exceptions;

public class EnrollmentStatusHistoryService : IEnrollmentStatusHistoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public EnrollmentStatusHistoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<EnrollmentStatusHistory> AddStatusAsync(EnrollmentStatusHistoryUpdateDto dto)
    {
        var enrollment = await _uow.Enrollments.GetByIdAsync(dto.EnrollmentId)
                         ?? throw new NotFoundException($"Enrollment {dto.EnrollmentId} not found.");

        var history = _mapper.Map<EnrollmentStatusHistory>(dto);

        _uow.BeginTransaction();
        try
        {
            await _uow.EnrollmentStatusHistories.AddAsync(history);
            enrollment.Status = dto.NewStatus;
            await _uow.Enrollments.UpdateAsync(enrollment);

            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }

        return history;
    }

    public async Task<IEnumerable<EnrollmentStatusHistory>> GetByEnrollmentAsync(int enrollmentId)
    {
        return await _uow.EnrollmentStatusHistories.GetByEnrollmentAsync(enrollmentId);
    }
}
