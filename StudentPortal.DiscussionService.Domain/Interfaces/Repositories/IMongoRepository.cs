
using System.Linq.Expressions;

namespace StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
    public interface IMongoRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    }
