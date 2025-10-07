namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;


    public interface IModuleService
    {
        Task<Module> CreateModuleAsync(Module module, CancellationToken cancellationToken = default);
        Task<Module> UpdateModuleAsync(Module module, CancellationToken cancellationToken = default);
        Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default);

        Task<Module?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId, CancellationToken cancellationToken = default);
        
    }
