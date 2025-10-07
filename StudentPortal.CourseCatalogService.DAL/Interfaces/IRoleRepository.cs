namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;

    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
  
    }
