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
using StudentPortal.ServiceDefaults.Metrics;
using System.Diagnostics;

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
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.List, 
                async () =>
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

                    CourseMetrics.RecordFetch(MetricConstants.Values.List, cachedList != null && cachedList.Any()); 

                    if (cachedList != null && cachedList.Any())
                        _logger.LogInformation("Cache HIT for key: {CacheKey} | ItemsCount: {Count}", cacheKey, cachedList.Count);

                    return new PagedList<CourseDto>(cachedList ?? new List<CourseDto>(), cachedList?.Count ?? 0, parameters.Page, parameters.PageSize);
                });
        }

        public async Task<CourseDetailsDto> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.GetById,
                async () =>
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

                    CourseMetrics.RecordFetch(MetricConstants.Values.GetById, course != null);
                    return course;
                });
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:all",
                async () =>
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

                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:all", cached != null);
                    return cached ?? new List<CourseDto>();
                });
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(int instructorId, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:by_instructor",
                async () =>
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

                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:by_instructor", cached != null);
                    return cached ?? new List<CourseDto>();
                });
        }

        public async Task<IEnumerable<CourseDto>> SearchCoursesAsync(string keyword, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:search",
                async () =>
                {
                    var courses = await _unitOfWork.Courses.SearchCoursesAsync(keyword);
                    var result = _mapper.Map<IEnumerable<CourseDto>>(courses);
                    
                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:search", result.Any());
                    return result;
                });
        }

        public async Task<IEnumerable<CourseDto>> GetPublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:published",
                async () =>
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

                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:published", cached != null);
                    return cached ?? new List<CourseDto>();
                });
        }

        public async Task<IEnumerable<CourseDto>> GetUnpublishedCoursesAsync(CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:unpublished",
                async () =>
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

                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:unpublished", cached != null);
                    return cached ?? new List<CourseDto>();
                });
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesWithMoreThanNStudentsAsync(int count, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                $"{MetricConstants.Values.List}:more_than_n_students",
                async () =>
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

                    CourseMetrics.RecordFetch($"{MetricConstants.Values.List}:more_than_n_students", cached != null);
                    return cached ?? new List<CourseDto>();
                });
        }

        public async Task<CourseDto> CreateCourseAsync(CourseCreateDto dto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Create,
                async () =>
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

                    CourseMetrics.CoursesCreated.Add(1, MetricConstants.Tags.OperationCreate);
                    return createdDto;
                });
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateDto dto, CancellationToken cancellationToken = default)
        {
            return await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Update,
                async () =>
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
                    await _cacheService.RemoveAsync(cacheKey);

                    CourseMetrics.CoursesUpdated.Add(1, MetricConstants.Tags.OperationUpdate);
                    return updatedDto;
                });
        }

        public async Task DeleteCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Delete,
                async () =>
                {
                    var course = await _unitOfWork.Courses.GetByIdAsync(id, cancellationToken: cancellationToken);
                    if (course is null)
                        throw new NotFoundException($"Course with ID {id} not found.");

                    await _unitOfWork.Courses.DeleteAsync(id, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    CourseMetrics.CoursesDeleted.Add(1, MetricConstants.Tags.OperationDelete);
                });
        }

        public async Task PublishCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Publish,
                async () =>
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

                    CourseMetrics.CoursesUpdated.Add(1, new TagList { new(MetricConstants.Keys.Operation, MetricConstants.Values.Publish) });
                });
        }

        public async Task UnpublishCourseAsync(int id, CancellationToken cancellationToken = default)
        {
            await MetricRecorder.RecordOperationAsync(
                CourseMetrics.OperationLatency,
                MetricConstants.Values.Unpublish,
                async () =>
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

                    CourseMetrics.CoursesUpdated.Add(1, new TagList { new(MetricConstants.Keys.Operation, MetricConstants.Values.Unpublish) });
                });
        }

        private static string GenerateCoursesListCacheKey(CourseParameters parameters)
        {
            return $"courses:page:{parameters.Page}:size:{parameters.PageSize}:order:{parameters.OrderBy ?? "Id"}:title:{parameters.Title ?? ""}:instructor:{parameters.InstructorId?.ToString() ?? ""}";
        }
    }
}