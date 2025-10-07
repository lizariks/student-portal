using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IStudentCourseRepository _studentCourseRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StudentCourseService(
            IStudentCourseRepository studentCourseRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _studentCourseRepository = studentCourseRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StudentCourse> EnrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new NotFoundException($"Course with id {courseId} not found.");

            if (!course.IsPublished)
                throw new BusinessException("Cannot enroll in an unpublished course.");

            bool alreadyEnrolled = await _studentCourseRepository.IsUserEnrolledAsync(userId, courseId);
            if (alreadyEnrolled)
                throw new BusinessException("Student is already enrolled in this course.");

            var enrollment = new StudentCourse
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _studentCourseRepository.AddAsync(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return enrollment;
        }

        public async Task UnenrollStudentAsync(int userId, int courseId, CancellationToken cancellationToken = default)
        {
            var enrollment = await _studentCourseRepository.GetEnrollmentAsync(userId, courseId);
            if (enrollment == null)
                throw new NotFoundException("Enrollment not found.");

            await _studentCourseRepository.DeleteAsync(enrollment.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<StudentCourse>> GetEnrollmentsByUserAsync(int userId)
        {
            return await _studentCourseRepository.GetEnrollmentsByUserAsync(userId);
        }

        public async Task<IEnumerable<StudentCourse>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _studentCourseRepository.GetEnrollmentsByCourseAsync(courseId);
        }

        public async Task<int> GetEnrollmentCountForCourseAsync(int courseId)
        {
            return await _studentCourseRepository.GetEnrollmentCountForCourseAsync(courseId);
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await _studentCourseRepository.IsUserEnrolledAsync(userId, courseId);
        }
    }
}
