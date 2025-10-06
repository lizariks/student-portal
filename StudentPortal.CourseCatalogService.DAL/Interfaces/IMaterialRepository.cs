namespace StudentPortal.CourseCatalogService.DAL.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;


    public interface IMaterialRepository : IGenericRepository<Material>
    {
        Task<Material?> GetMaterialWithLessonAsync(int materialId);
        Task LoadLessonAsync(Material material);
        Task<IEnumerable<Material>> GetMaterialsByLessonAsync(int lessonId);
        Task<IEnumerable<Material>> GetMaterialsByTypeAsync(MaterialType type);
        Task<IEnumerable<Material>> GetMaterialsWithoutUrlAsync();
        Task<IEnumerable<Material>> GetOrderedMaterialsByLessonAsync(int lessonId);
    }
