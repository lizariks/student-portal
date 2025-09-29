namespace StudentPortal.Enrollment.DAL.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;
        private const string TableName = "Enrollments";

        public EnrollmentRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName}";
            return await _connection.QueryAsync<Enrollment>(new CommandDefinition(sql, transaction: _transaction, cancellationToken: ct));
        }

        public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName} WHERE EnrollmentId = @Id";
            return await _connection.QueryFirstOrDefaultAsync<Enrollment>(
                new CommandDefinition(sql, new { Id = id }, transaction: _transaction, cancellationToken: ct));
        }

        public async Task AddAsync(Enrollment entity, CancellationToken ct = default)
        {
            var sql = $"INSERT INTO {TableName} (StudentId, CourseId, EnrolledAt, Status) " +
                      "VALUES (@StudentId, @CourseId, @EnrolledAt, @Status)";
            await _connection.ExecuteAsync(
                new CommandDefinition(sql, entity, transaction: _transaction, cancellationToken: ct));
        }

        public async Task UpdateAsync(Enrollment entity, CancellationToken ct = default)
        {
            var sql = $"UPDATE {TableName} SET StudentId = @StudentId, CourseId = @CourseId, " +
                      "EnrolledAt = @EnrolledAt, Status = @Status WHERE EnrollmentId = @EnrollmentId";
            await _connection.ExecuteAsync(
                new CommandDefinition(sql, entity, transaction: _transaction, cancellationToken: ct));
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var sql = $"DELETE FROM {TableName} WHERE EnrollmentId = @Id";
            await _connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = id }, transaction: _transaction, cancellationToken: ct));
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId, CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName} WHERE StudentId = @StudentId";
            return await _connection.QueryAsync<Enrollment>(
                new CommandDefinition(sql, new { StudentId = studentId }, transaction: _transaction, cancellationToken: ct));
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId, CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName} WHERE CourseId = @CourseId";
            return await _connection.QueryAsync<Enrollment>(
                new CommandDefinition(sql, new { CourseId = courseId }, transaction: _transaction, cancellationToken: ct));
        }
        
    }
