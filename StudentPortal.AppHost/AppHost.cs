using MongoDB.Driver;
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d").WithPgAdmin();

/**var mongo = builder.AddMongoDB("mongodb")
    .WithDataVolume();**/

var enrollmentDb = postgres.AddDatabase("EnrollmentDb");
//var discussionDb = mongo.AddDatabase("studentportal-discussion-service-db");

//builder.AddProject<Projects.StudentPortal_DiscussionService_Api>("discussionservice-api")
    //.WithReference(discussionDb)
    //.WaitFor(discussionDb);

builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollment-api")
    .WithReference(enrollmentDb)
    .WaitFor(enrollmentDb);

var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");

builder.AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

await builder.Build().RunAsync();