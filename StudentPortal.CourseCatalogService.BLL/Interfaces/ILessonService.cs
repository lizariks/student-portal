namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


    public interface ILessonService
    {
        Task<IEnumerable<LessonDto>> GetAllLessonsAsync(CancellationToken cancellationToken = default);
        Task<LessonDto> GetLessonByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<LessonDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken cancellationToken = default);

        Task<LessonDto> CreateLessonAsync(LessonCreateDto dto, CancellationToken cancellationToken = default);
        Task<LessonDto> UpdateLessonAsync(int id, LessonUpdateDto dto, CancellationToken cancellationToken = default);
        Task DeleteLessonAsync(int id, CancellationToken cancellationToken = default);

        Task ReorderLessonAsync(int lessonId, int newOrder, CancellationToken cancellationToken = default);
    }
