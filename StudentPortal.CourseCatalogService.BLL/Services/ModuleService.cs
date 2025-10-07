using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ModuleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedList<ModuleDto>> GetPagedModulesAsync(ModuleParameters parameters,
            CancellationToken cancellationToken = default)
        {
            var pagedModules = await _unitOfWork.Modules.GetPagedModulesAsync(parameters, cancellationToken);
            var mappedItems = _mapper.Map<IEnumerable<ModuleDto>>(pagedModules);

            return new PagedList<ModuleDto>(
                mappedItems.ToList(),
                pagedModules.TotalCount,
                pagedModules.Page,
                pagedModules.PageSize
            );
        }

        public async Task<ModuleDto?> GetModuleWithLessonsAsync(int moduleId,
            CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork.Modules.GetModuleWithLessonsAsync(moduleId, cancellationToken);
            return module == null ? null : _mapper.Map<ModuleDto>(module);
        }

        public async Task<IEnumerable<ModuleDto>> GetModulesByCourseAsync(int courseId,
            CancellationToken cancellationToken = default)
        {
            var modules = await _unitOfWork.Modules.GetModulesByCourseAsync(courseId, cancellationToken);
            return _mapper.Map<IEnumerable<ModuleDto>>(modules);
        }

        public async Task<ModuleDto> CreateModuleAsync(ModuleDto moduleDto,
            CancellationToken cancellationToken = default)
        {
            if (moduleDto == null)
                throw new BusinessException("Module cannot be null.");

            var course = await _unitOfWork.Courses.GetByIdAsync(moduleDto.CourseId, true, cancellationToken);
            if (course == null)
                throw new NotFoundException($"Course with id {moduleDto.CourseId} not found.");

            var module = _mapper.Map<Module>(moduleDto);
            await _unitOfWork.Modules.AddAsync(module, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ModuleDto>(module);
        }

        public async Task<ModuleDto> UpdateModuleAsync(ModuleDto moduleDto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Modules.GetByIdAsync(moduleDto.Id, false, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Module with id {moduleDto.Id} not found.");

            _mapper.Map(moduleDto, existing);

            await _unitOfWork.Modules.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ModuleDto>(existing);
        }

        public async Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(moduleId, false, cancellationToken);
            if (module == null)
                throw new NotFoundException($"Module with id {moduleId} not found.");

            await _unitOfWork.Modules.DeleteAsync(moduleId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
