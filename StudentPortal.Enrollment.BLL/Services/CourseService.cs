namespace StudentPortal.Enrollment.BLL.Services;

using AutoMapper;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.Domain.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(IUnitOfWork uow, IMapper mapper,  ILogger<CourseService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Course> GetByIdAsync(int id)
        {
            var course = await _uow.Courses.GetByIdAsync(id);
            if (course == null)
                throw new NotFoundException($"Course with id {id} not found.");
            return course;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _uow.Courses.GetAllAsync();
        }

        public async Task<Course> CreateAsync(Course course)
        {
            var existing = await _uow.Courses.GetAllAsync();
            if (existing.Any(c => c.Title == course.Title))
                throw new BusinessConflictException($"Course with title '{course.Title}' already exists.");

            _uow.BeginTransaction();
            try
            {
                await _uow.Courses.AddAsync(course);
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }

            return course;
        }

        public async Task UpdateAsync(Course course)
        {
            var existing = await _uow.Courses.GetByIdAsync(course.CourseId);
            if (existing == null)
                throw new NotFoundException($"Course with id {course.CourseId} not found.");

            _uow.BeginTransaction();
            try
            {
                await _uow.Courses.UpdateAsync(course);
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
            var existing = await _uow.Courses.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Course with id {id} not found.");

            _uow.BeginTransaction();
            try
            {
                await _uow.Courses.DeleteAsync(id);
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync()
        {
            return await _uow.Courses.GetCoursesWithEnrollmentsAsync();
        }
    }
