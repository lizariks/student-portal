using Ardalis.Specification;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.CourseCatalogService.DAL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetQueryable();
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec,
            CancellationToken cancellationToken = default);
    }
}