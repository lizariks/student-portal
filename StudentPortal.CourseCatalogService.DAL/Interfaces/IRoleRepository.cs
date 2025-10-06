namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;

    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
    }
