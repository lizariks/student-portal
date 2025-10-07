
using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentCourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StudentCourseDto> EnrollStudentAsync(
            StudentCourseCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId, true, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id {dto.UserId} not found.");

            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId, true, cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with id {dto.CourseId} not found.");

            if (!course.IsPublished)
                throw new BusinessException("Cannot enroll in an unpublished course.");

            bool alreadyEnrolled = await _unitOfWork.StudentCourses.IsUserEnrolledAsync(dto.UserId, dto.CourseId);
            if (alreadyEnrolled)
                throw new BusinessException("Student is already enrolled in this course.");

            var enrollment = _mapper.Map<StudentCourse>(dto);
            enrollment.EnrolledAt = DateTime.UtcNow;

            await _unitOfWork.StudentCourses.AddAsync(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<StudentCourseDto>(enrollment);
        }

        public async Task UnenrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default)
        {
            var enrollment = await _unitOfWork.StudentCourses.GetEnrollmentAsync(userId, courseId);
            if (enrollment == null)
                throw new NotFoundException("Enrollment not found.");

            await _unitOfWork.StudentCourses.DeleteAsync(enrollment.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<PagedList<StudentCourseListDto>> GetPagedStudentCoursesAsync(
            StudentCourseParameters parameters,
            CancellationToken cancellationToken = default)
        {
            var pagedEnrollments = await _unitOfWork.StudentCourses.GetPagedStudentCoursesAsync(parameters, cancellationToken);
            var mappedItems = _mapper.Map<IEnumerable<StudentCourseListDto>>(pagedEnrollments);

            return new PagedList<StudentCourseListDto>(
                mappedItems.ToList(),
                pagedEnrollments.TotalCount,
                pagedEnrollments.Page,
                pagedEnrollments.PageSize
            );
        }

        public async Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByUserAsync(int userId)
        {
            var enrollments = await _unitOfWork.StudentCourses.GetEnrollmentsByUserAsync(userId);
            return _mapper.Map<IEnumerable<StudentCourseDto>>(enrollments);
        }

        public async Task<IEnumerable<StudentCourseDto>> GetEnrollmentsByCourseAsync(int courseId)
        {
            var enrollments = await _unitOfWork.StudentCourses.GetEnrollmentsByCourseAsync(courseId);
            return _mapper.Map<IEnumerable<StudentCourseDto>>(enrollments);
        }

        public async Task<int> GetEnrollmentCountForCourseAsync(int courseId)
        {
            return await _unitOfWork.StudentCourses.GetEnrollmentCountForCourseAsync(courseId);
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await _unitOfWork.StudentCourses.IsUserEnrolledAsync(userId, courseId);
        }
    }
}
