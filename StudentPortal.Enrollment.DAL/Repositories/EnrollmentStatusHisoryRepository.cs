namespace StudentPortal.Enrollment.DAL.Repositories;

using Dapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class EnrollmentStatusHistoryRepository 
    : GenericRepository<EnrollmentStatusHistory>, IEnrollmentStatusHistoryRepository
{
    public EnrollmentStatusHistoryRepository(IDbConnection connection, IDbTransaction? transaction = null)
        : base("enrollment_status_history", connection, transaction) { }

    public async Task<IEnumerable<EnrollmentStatusHistory>> GetByEnrollmentAsync(int enrollmentId, CancellationToken ct = default)
    {
        var sql = "SELECT * FROM enrollment_status_history WHERE enrollment_id = @EnrollmentId";
        return await _connection.QueryAsync<EnrollmentStatusHistory>(
            new CommandDefinition(sql, new { EnrollmentId = enrollmentId }, _transaction, cancellationToken: ct));
    }
}