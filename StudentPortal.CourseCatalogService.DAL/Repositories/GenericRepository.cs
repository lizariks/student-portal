namespace StudentPortal.CourseCatalogService.DAL.Repositories;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        protected readonly CourseCatalogDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public IQueryable<TEntity> GetQueryable() => _dbSet.AsQueryable();

        public GenericRepository(CourseCatalogDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
            return await query.ToListAsync(cancellationToken);
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, asNoTracking: false, cancellationToken);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            var evaluator = new SpecificationEvaluator();
            var query = evaluator.GetQuery(_dbSet.AsQueryable(), spec).AsNoTracking();
            return await query.ToListAsync(cancellationToken);
        }

        protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            var evaluator = new SpecificationEvaluator();
            return evaluator
                .GetQuery(_dbSet.AsQueryable(), spec)
                .AsSplitQuery();
        }
    }
