namespace StudentPortal.CourseCatalogService.BLL.Services;

using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<LessonDto> CreateLessonAsync(LessonCreateDto dto, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(dto.ModuleId, cancellationToken: cancellationToken);
            if (module == null) throw new NotFoundException($"Module with Id {dto.ModuleId} not found");

            var lesson = _mapper.Map<Lesson>(dto);
            await _unitOfWork.Lessons.AddAsync(lesson, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task DeleteLessonAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

            await _unitOfWork.Lessons.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync(CancellationToken cancellationToken = default)
        {
            var lessons = await _unitOfWork.Lessons.GetAllAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<LessonDto> GetLessonByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(moduleId, cancellationToken: cancellationToken);
            if (module == null) throw new NotFoundException($"Module with Id {moduleId} not found");

            var lessons = module.Lessons.OrderBy(l => l.Order).ToList();
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task<LessonDto> UpdateLessonAsync(int id, LessonUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {id} not found");

            _mapper.Map(dto, lesson);
            await _unitOfWork.Lessons.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task ReorderLessonAsync(int lessonId, int newOrder, CancellationToken cancellationToken = default)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId, asNoTracking: false, cancellationToken: cancellationToken);
            if (lesson == null) throw new NotFoundException($"Lesson with Id {lessonId} not found");

            var moduleLessons = (await _unitOfWork.Lessons.GetAllAsync(cancellationToken: cancellationToken))
                                .Where(l => l.ModuleId == lesson.ModuleId && l.Id != lesson.Id)
                                .OrderBy(l => l.Order)
                                .ToList();

            moduleLessons.Insert(newOrder - 1, lesson);
            for (int i = 0; i < moduleLessons.Count; i++)
                moduleLessons[i].Order = i + 1;

            foreach (var l in moduleLessons)
                await _unitOfWork.Lessons.UpdateAsync(l);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
