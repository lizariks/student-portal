namespace StudentPortal.CourseCatalogService.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
public class ModuleRepository : GenericRepository<Module>, IModuleRepository
{
    public ModuleRepository(CourseCatalogDbContext context) : base(context)
    {
    }

    public async Task<Module?> GetModuleWithLessonsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Lessons)
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);
    }

    public async Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId)
    {
        return await _dbSet
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .ToListAsync();
    }

    public async Task ReorderModulesAsync(int courseId, List<int> orderedModuleIds, CancellationToken cancellationToken = default)
    {
        var modules = await _dbSet
            .Where(m => m.CourseId == courseId)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < orderedModuleIds.Count; i++)
        {
            var module = modules.FirstOrDefault(m => m.Id == orderedModuleIds[i]);
            if (module != null)
                module.Order = i + 1;
        }

        _context.UpdateRange(modules);
    }
}