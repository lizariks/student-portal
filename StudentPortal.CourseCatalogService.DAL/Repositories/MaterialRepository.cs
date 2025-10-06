namespace StudentPortal.CourseCatalogService.DAL.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;


    public class MaterialRepository : GenericRepository<Material>, IMaterialRepository
    {
        private readonly CourseCatalogDbContext _context;

        public MaterialRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Material?> GetMaterialWithLessonAsync(int materialId)
        {
            return await _context.Materials
                .Include(m => m.Lesson)
                .FirstOrDefaultAsync(m => m.Id == materialId);
        }

        public async Task LoadLessonAsync(Material material)
        {
            if (material == null) return;
            await _context.Entry(material)
                .Reference(m => m.Lesson)
                .LoadAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsByLessonAsync(int lessonId)
        {
            return await _context.Materials
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsByTypeAsync(MaterialType type)
        {
            return await _context.Materials
                .Where(m => m.Type == type)
                .Include(m => m.Lesson)
                .ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsWithoutUrlAsync()
        {
            return await _context.Materials
                .Where(m => string.IsNullOrEmpty(m.Url))
                .Include(m => m.Lesson)
                .ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetOrderedMaterialsByLessonAsync(int lessonId)
        {
            return await _context.Materials
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.Order)
                .Include(m => m.Lesson)
                .ToListAsync();
        }
    }