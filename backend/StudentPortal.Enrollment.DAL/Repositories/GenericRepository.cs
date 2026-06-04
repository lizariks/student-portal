namespace StudentPortal.Enrollment.DAL.Repositories;
using Dapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using StudentPortal.Enrollment.Domain;
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly string _tableName;
        protected readonly IDbConnection _connection;
        protected readonly IDbTransaction? _transaction;

        protected GenericRepository(string tableName, IDbConnection connection, IDbTransaction? transaction = null)
        {
            _tableName = tableName;
            _connection = connection;
            _transaction = transaction;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            var query = $"SELECT * FROM {_tableName}";
            return await _connection.QueryAsync<T>(new CommandDefinition(query, transaction: _transaction, cancellationToken: ct));
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var query = $"SELECT * FROM {_tableName} WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: ct));
        }

        public virtual async Task AddAsync(T entity,CancellationToken ct = default)
        {
            var sql = GenerateInsertQuery();
            await _connection.ExecuteAsync(sql, entity, _transaction);
        }

        public virtual async Task UpdateAsync(T entity,CancellationToken ct = default)
        {
            var sql = GenerateUpdateQuery();
            await _connection.ExecuteAsync(sql, entity, _transaction);
        }

        public virtual async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var query = $"DELETE FROM {_tableName} WHERE id = @Id";
            await _connection.ExecuteAsync(new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: ct));
        }
        
        protected string GenerateInsertQuery()
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.Name != "Id")
                .Select(p => p.Name);

            var columns = string.Join(", ", props);
            var values = string.Join(", ", props.Select(p => "@" + p));

            return $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";
        }

        protected string GenerateUpdateQuery()
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.Name != "Id")
                .Select(p => $"{p.Name} = @{p.Name}");

            var setClause = string.Join(", ", props);
            return $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";
        }
    }

