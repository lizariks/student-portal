namespace StudentPortal.CourseCatalogService.DAL.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;

public interface ILessonRepository : IGenericRepository<Lesson>
{
    Task<PagedList<Lesson>> GetPagedLessonsAsync(
        LessonParameters parameters,
        ISortHelper<Lesson>? sortHelper = null,
        CancellationToken cancellationToken = default);

    Task<Lesson?> GetLessonWithMaterialsExplicitAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<Lesson?> GetLessonWithDetailsAsync(int lessonId);

    Task LoadMaterialsAsync(Lesson lesson);
    Task LoadModuleAsync(Lesson lesson);

    Task<IEnumerable<Lesson>> GetLessonsByModuleAsync(int moduleId);
    Task<IEnumerable<Lesson>> GetLessonsByDurationRangeAsync(TimeSpan min, TimeSpan max);
    Task<IEnumerable<Lesson>> GetLessonsWithoutMaterialsAsync();
    Task<IEnumerable<Lesson>> GetLessonsWithMaterialsAsync();
    Task<IEnumerable<Lesson>> GetOrderedLessonsInModuleAsync(int moduleId);
}