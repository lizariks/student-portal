namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
    using StudentPortal.CourseCatalogService.DAL.Helpers;

    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<PagedList<Role>> GetPagedRolesAsync(RoleParameters parameters,
            CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string name);
  
    }
