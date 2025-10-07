namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;

    public interface IUserRepository :  IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetInstructorsAsync();
        Task<IEnumerable<User>> GetStudentsAsync();
    }
