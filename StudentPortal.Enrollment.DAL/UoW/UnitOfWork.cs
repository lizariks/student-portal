using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.DAL.Repositories;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.Enrollment.DAL.Connection;
namespace StudentPortal.Enrollment.DAL.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IConnectionFactory _connectionFactory;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed = false;
        private readonly object _lockObject = new object();

        public IStudentRepository? Students { get; private set; }
        public IEnrollmentRepository? Enrollments { get; private set; }
        public IEnrollmentStatusHistoryRepository? EnrollmentStatusHistories { get; private set; }
        public ICourseRepository? Courses { get; private set; }

        public IConnectionFactory ConnectionFactory => _connectionFactory;

        public UnitOfWork(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            InitializeRepositories();
        }

        private void InitializeRepositories()
        {
            if (_connection == null) throw new InvalidOperationException("Connection is null");

            Students = new StudentRepository(_connection, _transaction);
            Enrollments = new EnrollmentRepository(_connection, _transaction);
            EnrollmentStatusHistories = new EnrollmentStatusHistoryRepository(_connection, _transaction);
            Courses = new CourseRepository(_connection, _transaction);
        }

        private void EnsureTransactionStarted()
        {
            lock (_lockObject)
            {
                if (_transaction == null)
                {
                    if (_connection?.State != ConnectionState.Open)
                        _connection?.Open();

                    _transaction = _connection!.BeginTransaction();
                    InitializeRepositories();
                }
            }
        }

        public void BeginTransaction()
        {
            lock (_lockObject)
            {
                if (_transaction != null)
                    throw new InvalidOperationException("Transaction already started");

                EnsureTransactionStarted();
            }
        }

        public Task CommitAsync(CancellationToken ct = default)
        {
            lock (_lockObject)
            {
                EnsureTransactionStarted();

                if (_transaction == null)
                    throw new InvalidOperationException("No active transaction");

                try
                {
                    _transaction.Commit();
                }
                catch
                {
                    _transaction.Rollback();
                    throw;
                }
                finally
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }

            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken ct = default)
        {
            lock (_lockObject)
            {
                if (_transaction == null)
                    return Task.CompletedTask;

                try
                {
                    _transaction.Rollback();
                }
                finally
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (_transaction != null)
            {
                await RollbackAsync();
            }

            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
                _connection.Dispose();
                _connection = null;
            }

            _disposed = true;
        }
    }
}
