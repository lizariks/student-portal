namespace StudentPortal.CourseCatalogService.BLL.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;


    public interface IMaterialService
    {
        Task<PagedList<MaterialDto>> GetPagedMaterialsAsync(MaterialParameters parameters, CancellationToken cancellationToken = default);

        Task<MaterialDto> CreateMaterialAsync(MaterialDto materialDto, CancellationToken cancellationToken = default);

        Task<MaterialDto> UpdateMaterialAsync(int id, MaterialDto materialDto,
            CancellationToken cancellationToken = default);
        Task DeleteMaterialAsync(int materialId, CancellationToken cancellationToken = default);

        Task<MaterialDto?> GetMaterialWithLessonAsync(int materialId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaterialDto>> GetMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaterialDto>> GetMaterialsByTypeAsync(MaterialType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaterialDto>> GetMaterialsWithoutUrlAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MaterialDto>> GetOrderedMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default);

        Task ReorderMaterialsAsync(int lessonId, List<int> orderedMaterialIds, CancellationToken cancellationToken = default);
    }
