using Aspire;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Npgsql;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d").WithPgAdmin();;

var enrollmentDb = postgres.AddDatabase("EnrollmentDb");

builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollment-api")
    .WithReference(enrollmentDb)
    .WaitFor(enrollmentDb);

var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");

builder.AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

await builder.Build().RunAsync();