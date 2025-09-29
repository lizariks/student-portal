using Aspire;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Npgsql;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(); 

var enrollmentDb = postgres.AddDatabase("EnrollmentDb");

builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollment-api")
    .WithReference(enrollmentDb)
    .WaitFor(enrollmentDb);

builder.Build().Run();