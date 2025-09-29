namespace StudentPortal.Enrollment.BLL.Services;

using AutoMapper;
using StudentPortal.Enrollment.BLL.DTOs;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.Domain.Exceptions;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StudentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Student> GetByIdAsync(int id)
    {
        var student = await _uow.Students.GetByIdAsync(id)
                      ?? throw new NotFoundException($"Student {id} not found.");
        return student;
    }

    public async Task<Student> GetByEmailAsync(string email)
    {
        var student = await _uow.Students.GetByEmailAsync(email)
                      ?? throw new NotFoundException($"Student with email {email} not found.");
        return student;
    }

    public async Task<Student> CreateAsync(Student student)
    {
        _uow.BeginTransaction();
        try
        {
            await _uow.Students.AddAsync(student);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        _uow.BeginTransaction();
        try
        {
            await _uow.Students.UpdateAsync(student);
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
            await _uow.Students.DeleteAsync(id);
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
