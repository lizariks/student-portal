namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);

        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetInstructorsAsync();
        Task<IEnumerable<User>> GetStudentsAsync();
    }
