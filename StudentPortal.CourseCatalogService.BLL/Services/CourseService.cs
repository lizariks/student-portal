using AutoMapper;
using Microsoft.Extensions.Logging;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.ServiceDefaults.Hybrid;
using StudentPortal.CourseCatalogService.BLL.Metrics;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHybridCacheService _cacheService;
        private readonly ILogger<CourseService> _logger;
        private const string CachePrefix = "course";

        public CourseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHybridCacheService cacheService,
            ILogger<CourseService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PagedList<CourseDto>> GetPagedCoursesAsync(CourseParameters parameters, ISortHelper<Course>? sortHelper = null, CancellationToken cancellationToken = default)
        {
            string cacheKey = GenerateCoursesListCacheKey(parameters);
            var cachedList = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var pagedCourses = await _unitOfWork.Courses.GetPagedCoursesAsync(parameters, sortHelper, cancellationToken);
                    return _mapper.Map<List<CourseDto>>(pagedCourses);
                },
                memoryExpiration: TimeSpan.FromSeconds(30),
                redisExpiration: TimeSpan.FromMinutes(5)
            );

            if (cachedList != null && cachedList.Any())
                _logger.LogInformation("Cache HIT for key: {CacheKey} | ItemsCount: {Count}", cacheKey, cachedList.Count);

            CourseMetrics.CoursesFetched.Add(1);

            return new PagedList<CourseDto>(cachedList ?? new List<CourseDto>(), cachedList?.Count ?? 0, parameters.Page, parameters.PageSize);
        }

        public async Task<CourseDetailsDto> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{CachePrefix}:{id}";
            var course = await _cacheService.GetOrSetAsync<CourseDetailsDto>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var entity = await _unitOfWork.Courses.GetCourseWithDetailsAsync(id);
                    if (entity == null)
                        throw new NotFoundException($"Course with ID {id} not found.");
                    return _mapper.Map<CourseDetailsDto>(entity);
                },
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(30)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return course;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "courses:all";
            var cached = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var courses = await _unitOfWork.Courses.GetAllAsync(cancellationToken: cancellationToken);
                    return _mapper.Map<List<CourseDto>>(courses);
                },
                memoryExpiration: TimeSpan.FromMinutes(1),
                redisExpiration: TimeSpan.FromMinutes(10)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return cached ?? new List<CourseDto>();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(int instructorId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"courses:instructor:{instructorId}";
            var cached = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var courses = await _unitOfWork.Courses.GetCoursesByInstructorAsync(instructorId);
                    return _mapper.Map<List<CourseDto>>(courses);
                },
                memoryExpiration: TimeSpan.FromMinutes(1),
                redisExpiration: TimeSpan.FromMinutes(10)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return cached ?? new List<CourseDto>();
        }

        public async Task<IEnumerable<CourseDto>> SearchCoursesAsync(string keyword, CancellationToken cancellationToken = default)
        {
            var courses = await _unitOfWork.Courses.SearchCoursesAsync(keyword);
            CourseMetrics.CoursesFetched.Add(1);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetPublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "courses:published";
            var cached = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var courses = await _unitOfWork.Courses.GetPublishedCoursesAsync();
                    return _mapper.Map<List<CourseDto>>(courses);
                },
                memoryExpiration: TimeSpan.FromMinutes(1),
                redisExpiration: TimeSpan.FromMinutes(15)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return cached ?? new List<CourseDto>();
        }

        public async Task<IEnumerable<CourseDto>> GetUnpublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "courses:unpublished";
            var cached = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var courses = await _unitOfWork.Courses.GetUnpublishedCoursesAsync();
                    return _mapper.Map<List<CourseDto>>(courses);
                },
                memoryExpiration: TimeSpan.FromMinutes(1),
                redisExpiration: TimeSpan.FromMinutes(15)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return cached ?? new List<CourseDto>();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesWithMoreThanNStudentsAsync(int count, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"courses:students:morethan:{count}";
            var cached = await _cacheService.GetOrSetAsync<List<CourseDto>>(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
                    var courses = await _unitOfWork.Courses.GetCoursesWithMoreThanNStudentsAsync(count);
                    return _mapper.Map<List<CourseDto>>(courses);
                },
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(20)
            );

            CourseMetrics.CoursesFetched.Add(1);
            return cached ?? new List<CourseDto>();
        }

        public async Task<CourseDto> CreateCourseAsync(CourseCreateDto dto, CancellationToken cancellationToken = default)
        {
            var course = _mapper.Map<Course>(dto);
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.IsPublished = false;

            await _unitOfWork.Courses.AddAsync(course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdDto = _mapper.Map<CourseDto>(course);

            string cacheKey = $"{CachePrefix}:{course.Id}";
            await _cacheService.SetAsync(
                cacheKey,
                createdDto,
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(30)
            );

            CourseMetrics.CoursesCreated.Add(1);
            return createdDto;
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (course is null)
                throw new NotFoundException($"Course with ID {id} not found.");

            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedDto = _mapper.Map<CourseDto>(course);

            string cacheKey = $"{CachePrefix}:{id}";
            await _cacheService.SetAsync(
                cacheKey,
                updatedDto,
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(30)
            );

            CourseMetrics.CoursesUpdated.Add(1);
            return updatedDto;
        }

        public async Task DeleteCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (course is null)
                throw new NotFoundException($"Course with ID {id} not found.");

            await _unitOfWork.Courses.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            CourseMetrics.CoursesDeleted.Add(1);
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

            CourseMetrics.CoursesUpdated.Add(1);
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

            CourseMetrics.CoursesUpdated.Add(1);
        }

        private static string GenerateCoursesListCacheKey(CourseParameters parameters)
        {
            return $"courses:page:{parameters.Page}:size:{parameters.PageSize}:order:{parameters.OrderBy ?? "Id"}:title:{parameters.Title ?? ""}:instructor:{parameters.InstructorId?.ToString() ?? ""}";
        }
    }
}
