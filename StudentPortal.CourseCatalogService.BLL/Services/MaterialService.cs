using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedList<MaterialDto>> GetPagedMaterialsAsync(MaterialParameters parameters, CancellationToken cancellationToken = default)
        {
            var pagedMaterials = await _unitOfWork.Materials.GetPagedAsync(parameters, cancellationToken);
            var mappedItems = _mapper.Map<IEnumerable<MaterialDto>>(pagedMaterials);

            return new PagedList<MaterialDto>(
                mappedItems.ToList(),
                pagedMaterials.TotalCount,
                pagedMaterials.Page,
                pagedMaterials.PageSize);
        }

        public async Task<MaterialDto> CreateMaterialAsync(MaterialDto materialDto, CancellationToken cancellationToken = default)
        {
            if (materialDto == null)
                throw new BusinessException("Material cannot be null.");

            var material = _mapper.Map<Material>(materialDto);
            await _unitOfWork.Materials.AddAsync(material, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<MaterialDto>(material);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(int id, MaterialDto materialDto, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Materials.GetByIdAsync(id, false, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Material with id {id} not found.");

            _mapper.Map(materialDto, existing);

            await _unitOfWork.Materials.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<MaterialDto>(existing);
        }

        public async Task DeleteMaterialAsync(int id, CancellationToken cancellationToken = default)
        {
            var material = await _unitOfWork.Materials.GetByIdAsync(id, false, cancellationToken);
            if (material == null)
                throw new NotFoundException($"Material with id {id} not found.");

            await _unitOfWork.Materials.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<MaterialDto?> GetMaterialWithLessonAsync(int id, CancellationToken cancellationToken = default)
        {
            var material = await _unitOfWork.Materials.GetMaterialWithLessonAsync(id);
            return material == null ? null : _mapper.Map<MaterialDto>(material);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsByLessonAsync(lessonId);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByTypeAsync(MaterialType type, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsByTypeAsync(type);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsWithoutUrlAsync(CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsWithoutUrlAsync();
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<IEnumerable<MaterialDto>> GetOrderedMaterialsByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetOrderedMaterialsByLessonAsync(lessonId);
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task ReorderMaterialsAsync(int lessonId, List<int> orderedMaterialIds, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Materials.GetMaterialsByLessonAsync(lessonId);
            if (!orderedMaterialIds.All(id => materials.Any(m => m.Id == id)))
                throw new BusinessException("Ordered IDs do not match the materials in the lesson.");

            for (int i = 0; i < orderedMaterialIds.Count; i++)
            {
                var material = materials.First(m => m.Id == orderedMaterialIds[i]);
                material.Order = i + 1;
            }

            foreach (var material in materials)
                await _unitOfWork.Materials.UpdateAsync(material);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
