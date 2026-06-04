namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;


    public interface IModuleService
    {
        Task<ModuleDto> CreateModuleAsync(ModuleDto moduleDto, CancellationToken cancellationToken = default);
        Task<ModuleDto> UpdateModuleAsync(ModuleDto module, CancellationToken cancellationToken = default);
        Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default);

        Task<ModuleDto?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default);

        Task<IEnumerable<ModuleDto>> GetModulesByCourseAsync(int courseId,
            CancellationToken cancellationToken = default);
    }
