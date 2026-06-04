namespace StudentPortal.Enrollment.DAL.Connection;
using Npgsql;
using System.Data;
public interface IConnectionFactory
{
    IDbConnection CreateConnection();
}