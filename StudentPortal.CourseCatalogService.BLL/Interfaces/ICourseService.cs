using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.BLL.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseListDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default);
        Task<CourseDetailsDto> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseListDto>> GetCoursesByInstructorAsync(int instructorId, CancellationToken cancellationToken = default);

        Task<CourseDto> CreateCourseAsync(CourseCreateDto dto, CancellationToken cancellationToken = default);
        Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateDto dto, CancellationToken cancellationToken = default);
        Task DeleteCourseAsync(int id, CancellationToken cancellationToken = default);

        Task PublishCourseAsync(int id, CancellationToken cancellationToken = default);
        Task UnpublishCourseAsync(int id, CancellationToken cancellationToken = default);

        Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string keyword, CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseListDto>> GetPublishedCoursesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseListDto>> GetUnpublishedCoursesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseListDto>> GetCoursesWithMoreThanNStudentsAsync(int count, CancellationToken cancellationToken = default);
    }
}