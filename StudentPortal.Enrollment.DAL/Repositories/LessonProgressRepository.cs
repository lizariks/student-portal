namespace StudentPortal.Enrollment.DAL.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain.Entities;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;
    private const string TableName = "LessonProgress";

    public LessonProgressRepository(IDbConnection connection, IDbTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<LessonProgress>> GetAllAsync(CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM {TableName}";
        return await _connection.QueryAsync<LessonProgress>(new CommandDefinition(sql, transaction: _transaction, cancellationToken: ct));
    }

    public async Task<LessonProgress?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM {TableName} WHERE ProgressId = @Id";
        return await _connection.QueryFirstOrDefaultAsync<LessonProgress>(
            new CommandDefinition(sql, new { Id = id }, transaction: _transaction, cancellationToken: ct));
    }

    public async Task AddAsync(LessonProgress entity, CancellationToken ct = default)
    {
        var sql = $"INSERT INTO {TableName} (StudentId, LessonId, CourseId, CompletedAt) " +
                  "VALUES (@StudentId, @LessonId, @CourseId, @CompletedAt) " +
                  "ON CONFLICT (StudentId, LessonId) DO NOTHING";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, transaction: _transaction, cancellationToken: ct));
    }

    public Task UpdateAsync(LessonProgress entity, CancellationToken ct = default) => Task.CompletedTask;

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var sql = $"DELETE FROM {TableName} WHERE ProgressId = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, transaction: _transaction, cancellationToken: ct));
    }

    public async Task<IEnumerable<LessonProgress>> GetByStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM {TableName} WHERE StudentId = @StudentId AND CourseId = @CourseId";
        return await _connection.QueryAsync<LessonProgress>(
            new CommandDefinition(sql, new { StudentId = studentId, CourseId = courseId }, transaction: _transaction, cancellationToken: ct));
    }

    public async Task<IEnumerable<LessonProgress>> GetByCourseAsync(int courseId, CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM {TableName} WHERE CourseId = @CourseId";
        return await _connection.QueryAsync<LessonProgress>(
            new CommandDefinition(sql, new { CourseId = courseId }, transaction: _transaction, cancellationToken: ct));
    }

    public async Task DeleteByStudentAndLessonAsync(int studentId, int lessonId, CancellationToken ct = default)
    {
        var sql = $"DELETE FROM {TableName} WHERE StudentId = @StudentId AND LessonId = @LessonId";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { StudentId = studentId, LessonId = lessonId }, transaction: _transaction, cancellationToken: ct));
    }
}
