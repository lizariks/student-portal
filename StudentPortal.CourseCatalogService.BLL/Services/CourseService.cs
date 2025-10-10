using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        public async Task<PagedList<CourseDto>> GetPagedCoursesAsync(
            CourseParameters parameters,
            ISortHelper<Course>? sortHelper = null,
            CancellationToken cancellationToken = default)
        {
            var pagedCourses = await _unitOfWork.Courses.GetPagedCoursesAsync(parameters, sortHelper, cancellationToken);
            var mappedItems = _mapper.Map<IEnumerable<CourseDto>>(pagedCourses);
    
            return new PagedList<CourseDto>(
                mappedItems.ToList(),
                pagedCourses.TotalCount,
                pagedCourses.Page,
                pagedCourses.PageSize);
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.GetAllAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDetailsDto> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetCourseWithDetailsAsync(id);
            if (course == null)
                throw new NotFoundException($"Course with ID {id} was not found.");

            return _mapper.Map<CourseDetailsDto>(course);
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(int instructorId, CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.GetCoursesByInstructorAsync(instructorId);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto> CreateCourseAsync(CourseCreateDto dto, CancellationToken cancellationToken = default)
        {
            var course = _mapper.Map<Course>(dto);
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.IsPublished = false;

            await _unitOfWork.Courses.AddAsync(course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CourseDto>(course);
        }

        public async Task PublishCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            if (course.IsPublished)
                throw new BusinessException("Course is already published.");

            course.IsPublished = true;
            course.PublishedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UnpublishCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            if (!course.IsPublished)
                throw new BusinessException("Course is already unpublished.");

            course.IsPublished = false;
            course.PublishedAt = null;
            course.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            await _unitOfWork.Courses.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<CourseDto>> SearchCoursesAsync(string keyword, CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.SearchCoursesAsync(keyword);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetPublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.GetPublishedCoursesAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetUnpublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.GetUnpublishedCoursesAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesWithMoreThanNStudentsAsync(int count, CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.GetCoursesWithMoreThanNStudentsAsync(count);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }
    }
}
