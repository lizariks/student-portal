namespace StudentPortal.CourseCatalogService.DAL.Interfaces;


    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
    using StudentPortal.CourseCatalogService.DAL.Helpers;

    public interface IUserRepository :  IGenericRepository<User>
    {
        Task<PagedList<User>> GetPagedUsersAsync(UserParameters parameters, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetInstructorsAsync();
        Task<IEnumerable<User>> GetStudentsAsync();
    }
