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


    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ModuleService(
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IUnitOfWork unitOfWork)
        {
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Module> CreateModuleAsync(Module module, CancellationToken cancellationToken = default)
        {
            if (module == null)
                throw new BusinessException("Module cannot be null.");

            var course = await _courseRepository.GetByIdAsync(module.CourseId);
            if (course == null)
                throw new NotFoundException($"Course with id {module.CourseId} not found.");

            await _moduleRepository.AddAsync(module, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return module;
        }

        public async Task<Module> UpdateModuleAsync(Module module, CancellationToken cancellationToken = default)
        {
            var existing = await _moduleRepository.GetByIdAsync(module.Id, false, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"Module with id {module.Id} not found.");

            existing.Title = module.Title;
            existing.Description = module.Description;
            existing.Order = module.Order;

            await _moduleRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return existing;
        }

        public async Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            var module = await _moduleRepository.GetByIdAsync(moduleId, false, cancellationToken);
            if (module == null)
                throw new NotFoundException($"Module with id {moduleId} not found.");

            await _moduleRepository.DeleteAsync(moduleId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Module?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            return await _moduleRepository.GetModuleWithLessonsAsync(moduleId);
        }

        public async Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId, CancellationToken cancellationToken = default)
        {
            return await _moduleRepository.GetModulesByCourseAsync(courseId);
        }
        
    }
