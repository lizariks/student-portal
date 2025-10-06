using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.DAL.Data;
namespace StudentPortal.CourseCatalogService.DAL.Repositories
{
    public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
    {
        private readonly CourseCatalogDbContext _context;

        public LessonRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Lesson?> GetLessonWithDetailsAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Materials)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }
        public async Task<Lesson?> GetLessonWithMaterialsExplicitAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var lesson = await _dbSet.FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);
            if (lesson != null)
            {
                await _context.Entry(lesson)
                    .Collection(l => l.Materials)
                    .LoadAsync(cancellationToken);
            }
            return lesson;
        }//explicit loading

        public async Task LoadMaterialsAsync(Lesson lesson)
        {
            if (lesson == null) return;

            await _context.Entry(lesson)
                .Collection(l => l.Materials)
                .LoadAsync();
        }

        public async Task LoadModuleAsync(Lesson lesson)
        {
            if (lesson == null) return;

            await _context.Entry(lesson)
                .Reference(l => l.Module)
                .LoadAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByModuleAsync(int moduleId)
        {
            return await _context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByDurationRangeAsync(TimeSpan min, TimeSpan max)
        {
            return await _context.Lessons
                .Where(l => l.EstimatedDuration >= min && l.EstimatedDuration <= max)
                .Include(l => l.Module)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsWithoutMaterialsAsync()
        {
            return await _context.Lessons
                .Where(l => !l.Materials.Any())
                .Include(l => l.Module)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsWithMaterialsAsync()
        {
            return await _context.Lessons
                .Where(l => l.Materials.Any())
                .Include(l => l.Module)
                .Include(l => l.Materials)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetOrderedLessonsInModuleAsync(int moduleId)
        {
            return await _context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.Order)
                .Include(l => l.Materials)
                .ToListAsync();
        }
    }
}
