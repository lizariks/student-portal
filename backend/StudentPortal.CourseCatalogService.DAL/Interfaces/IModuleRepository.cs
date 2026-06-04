namespace StudentPortal.CourseCatalogService.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
public interface IModuleRepository : IGenericRepository<Module>
{
    
    Task<PagedList<Module>> GetPagedModulesAsync(ModuleParameters parameters, CancellationToken cancellationToken = default);
    Task<Module?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId,CancellationToken cancellationToken = default );
    Task ReorderModulesAsync(int courseId, List<int> orderedModuleIds, CancellationToken cancellationToken = default);
}