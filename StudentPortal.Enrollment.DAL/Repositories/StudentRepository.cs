using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;

namespace StudentPortal.Enrollment.DAL.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;
        private const string TableName = "Students";

        public StudentRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken ct = default)
        {
            var result = new List<Student>();
            var sql = $"SELECT * FROM {TableName}";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            using var reader = await ((DbCommand)command).ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(MapStudent(reader));
            }

            return result;
        }

        public async Task<Student?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName} WHERE StudentId = @Id";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            AddParameter(command, "@Id", id);

            using var reader = await ((DbCommand)command).ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                return MapStudent(reader);

            return null;
        }

        public async Task AddAsync(Student entity, CancellationToken ct = default)
        {
            var sql = $"INSERT INTO {TableName} (FirstName, LastName, Email) " +
                      "VALUES (@FirstName, @LastName, @Email)";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            AddParameter(command, "@FirstName", entity.FirstName);
            AddParameter(command, "@LastName", entity.LastName);
            AddParameter(command, "@Email", entity.Email);

            await ((DbCommand)command).ExecuteNonQueryAsync(ct);
        }

        public async Task UpdateAsync(Student entity, CancellationToken ct = default)
        {
            var sql = $"UPDATE {TableName} SET FirstName = @FirstName, LastName = @LastName, Email = @Email " +
                      "WHERE StudentId = @StudentId";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            AddParameter(command, "@FirstName", entity.FirstName);
            AddParameter(command, "@LastName", entity.LastName);
            AddParameter(command, "@Email", entity.Email);
            AddParameter(command, "@StudentId", entity.StudentId);

            await ((DbCommand)command).ExecuteNonQueryAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var sql = $"DELETE FROM {TableName} WHERE StudentId = @Id";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            AddParameter(command, "@Id", id);

            await ((DbCommand)command).ExecuteNonQueryAsync(ct);
        }
        
        public async Task<Student?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var sql = $"SELECT * FROM {TableName} WHERE Email = @Email";

            await EnsureConnectionOpenAsync(ct);

            using var command = CreateCommand(sql);
            AddParameter(command, "@Email", email);

            using var reader = await ((DbCommand)command).ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                return MapStudent(reader);

            return null;
        }
        
        private async Task EnsureConnectionOpenAsync(CancellationToken ct)
        {
            if (_connection.State != ConnectionState.Open)
                await ((SqlConnection)_connection).OpenAsync(ct);
        }

        private IDbCommand CreateCommand(string sql)
        {
            var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = _transaction;
            return command;
        }

        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        private static Student MapStudent(IDataReader reader)
        {
            return new Student
            {
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email"))
            };
        }
    }
}
