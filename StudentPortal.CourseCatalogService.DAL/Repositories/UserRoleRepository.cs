// StudentPortal.CourseCatalogService.DAL.Repositories/UserRoleRepository.cs
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.Extensions; // Для ApplySorting, ToPagedListAsync
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Specifications; // Тут має бути UserRoleWithFiltersSpecification
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.DAL.Repositories;

// UserRoleRepository успадковує GenericRepository<UserRole>
public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
{
    // Конструктор
    public UserRoleRepository(CourseCatalogDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Отримує сторінковий список UserRole, використовуючи шаблон Specification.
    /// </summary>
    public async Task<PagedList<UserRole>> GetRolesPagedAsync(
        UserRoleParameters parameters,
        ISortHelper<UserRole>? sortHelper = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Створюємо специфікацію для фільтрації.
        // NOTE: Вам потрібно створити клас UserRoleWithFiltersSpecification, 
        // аналогічний GameDeveloperRoleWithFiltersSpecification.
        var spec = new UserRoleWithFiltersSpecification(parameters); 
        
        // 2. Застосовуємо специфікацію (успадковано) та ApplySorting (метод розширення)
        var query = ApplySpecification(spec);

        // 3. Застосовуємо пагінацію (метод розширення)
        // Припускаємо, що ToPagedListAsync правильно обробляє IQueryable та об'єкт parameters.
        return await query.ToPagedListAsync(parameters, cancellationToken);
    }

    /// <summary>
    /// Отримує UserRole за композитним ключем із завантаженням зв'язків.
    /// </summary>
    public async Task<UserRole?> GetByCompositeKeyWithReferencesAsync(
        int userId, 
        int roleId, 
        CancellationToken cancellationToken = default)
    {
        // Пошук за композитним ключем
        var userRole = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
            
        if (userRole == null) return null;

        // Явне завантаження зв'язків (як у вашому прикладі)
        // _context та _dbSet доступні як protected поля у GenericRepository
        await _context.Entry(userRole).Reference(x => x.User).LoadAsync(cancellationToken);
        await _context.Entry(userRole).Reference(x => x.Role).LoadAsync(cancellationToken);

        return userRole;
    }
}