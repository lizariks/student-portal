namespace StudentPortal.CourseCatalogService.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
public interface IModuleRepository : IGenericRepository<Module>
{
    Task<Module?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId);
    Task ReorderModulesAsync(int courseId, List<int> orderedModuleIds, CancellationToken cancellationToken = default);
}