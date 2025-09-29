using Dapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace StudentPortal.Enrollment.DAL.Repositories
{
    public class CourseRepository 
        : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(IDbConnection connection, IDbTransaction? transaction = null)
            : base("courses", connection, transaction) { }
        
        public async Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync(CancellationToken ct = default)
        {
            var sql = @"
                SELECT DISTINCT c.*
                FROM Courses c
                LEFT JOIN Enrollments e ON c.CourseId = e.CourseId
                WHERE e.EnrollmentId IS NOT NULL";

            return await _connection.QueryAsync<Course>(
                new CommandDefinition(sql, transaction: _transaction, cancellationToken: ct));
        }
    }
}