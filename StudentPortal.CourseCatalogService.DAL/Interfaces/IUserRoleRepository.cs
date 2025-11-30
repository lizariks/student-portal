// StudentPortal.CourseCatalogService.DAL.Interfaces/IUserRoleRepository.cs
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Specifications; // Необхідно для коректної структури
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.DAL.Interfaces;

public interface IUserRoleRepository : IGenericRepository<UserRole>
{
    // Отримує сторінковий список UserRole
    Task<PagedList<UserRole>> GetRolesPagedAsync(
        UserRoleParameters parameters,
        ISortHelper<UserRole>? sortHelper = null,
        CancellationToken cancellationToken = default);

    // Отримує UserRole за композитним ключем із завантаженням зв'язків.
    // Оскільки UserRole не має єдиного Id, використовуємо UserId та RoleId
    Task<UserRole?> GetByCompositeKeyWithReferencesAsync(
        int userId, 
        int roleId, 
        CancellationToken cancellationToken = default);
}