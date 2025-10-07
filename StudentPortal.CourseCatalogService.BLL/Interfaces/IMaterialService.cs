namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;


    public interface IMaterialService
    {
        Task<Material> CreateMaterialAsync(Material material, CancellationToken cancellationToken = default);
        Task<Material> UpdateMaterialAsync(Material material, CancellationToken cancellationToken = default);
        Task DeleteMaterialAsync(int materialId, CancellationToken cancellationToken = default);

        Task<Material?> GetMaterialWithLessonAsync(int materialId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Material>> GetMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Material>> GetMaterialsByTypeAsync(MaterialType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<Material>> GetMaterialsWithoutUrlAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Material>> GetOrderedMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default);

        Task ReorderMaterialsAsync(int lessonId, List<int> orderedMaterialIds, CancellationToken cancellationToken = default);
    }
