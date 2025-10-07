namespace StudentPortal.CourseCatalogService.BLL.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;


    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialService(IMaterialRepository materialRepository, IUnitOfWork unitOfWork)
        {
            _materialRepository = materialRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Material> CreateMaterialAsync(Material material, CancellationToken cancellationToken = default)
        {
            if (material == null)
                throw new BusinessException("Material cannot be null.");

            await _materialRepository.AddAsync(material, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return material;
        }

        public async Task<Material> UpdateMaterialAsync(Material material, CancellationToken cancellationToken = default)
        {
            var existing = await _materialRepository.GetByIdAsync(material.Id, asNoTracking: false, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Material with id {material.Id} not found.");

            existing.Title = material.Title;
            existing.Url = material.Url;
            existing.Type = material.Type;
            existing.Order = material.Order;

            await _materialRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return existing;
        }

        public async Task DeleteMaterialAsync(int materialId, CancellationToken cancellationToken = default)
        {
            var material = await _materialRepository.GetByIdAsync(materialId, false, cancellationToken);
            if (material == null)
                throw new NotFoundException($"Material with id {materialId} not found.");

            await _materialRepository.DeleteAsync(materialId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Material?> GetMaterialWithLessonAsync(int materialId, CancellationToken cancellationToken = default)
        {
            return await _materialRepository.GetMaterialWithLessonAsync(materialId);
        }

        public async Task<IEnumerable<Material>> GetMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            return await _materialRepository.GetMaterialsByLessonAsync(lessonId);
        }

        public async Task<IEnumerable<Material>> GetMaterialsByTypeAsync(MaterialType type, CancellationToken cancellationToken = default)
        {
            return await _materialRepository.GetMaterialsByTypeAsync(type);
        }

        public async Task<IEnumerable<Material>> GetMaterialsWithoutUrlAsync(CancellationToken cancellationToken = default)
        {
            return await _materialRepository.GetMaterialsWithoutUrlAsync();
        }

        public async Task<IEnumerable<Material>> GetOrderedMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            return await _materialRepository.GetOrderedMaterialsByLessonAsync(lessonId);
        }

        public async Task ReorderMaterialsAsync(int lessonId, List<int> orderedMaterialIds, CancellationToken cancellationToken = default)
        {
            var materials = await _materialRepository.GetMaterialsByLessonAsync(lessonId);
            if (!orderedMaterialIds.All(id => materials.Any(m => m.Id == id)))
                throw new BusinessException("Ordered IDs do not match the materials in the lesson.");

            for (int i = 0; i < orderedMaterialIds.Count; i++)
            {
                var material = materials.First(m => m.Id == orderedMaterialIds[i]);
                material.Order = i + 1;
            }

            foreach (var material in materials)
                await _materialRepository.UpdateAsync(material);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
