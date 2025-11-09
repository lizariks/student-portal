
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d").WithPgAdmin();

var mongo = builder.AddMongoDB("mongodb").
    WithDataVolume();


var enrollmentDb = postgres.AddDatabase("EnrollmentDb");
var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");
var discussionDb = mongo.AddDatabase("studentportal-discussion-service-db");


/**var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisCommander();**/

 var discussionService= builder.AddProject<Projects.StudentPortal_DiscussionService_Api>("discussionservice-api")
    .WithReference(discussionDb)
    .WaitFor(discussionDb)
    .WithHttpEndpoint(port: 5003, name: "discussions-http")
     .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithHttpHealthCheck("/health"); ;

  var enrollmentService=builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollmentservice-api")
    .WithReference(enrollmentDb)
    .WaitFor(enrollmentDb)
    .WithHttpEndpoint(port: 5001, name: "enrollments-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);



var coursecatalogService=builder.AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpEndpoint(port: 5002, name: "coursescatalog-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

var aggregatorService = builder.AddProject<Projects.StudentPortal_AggregatorService>("aggregatorservice-api")
    .WithReference(enrollmentService)
    .WithReference(coursecatalogService)
    .WithReference(discussionService)
    .WaitFor(enrollmentService)
    .WaitFor(coursecatalogService)
    .WaitFor(discussionService)
    .WithHttpEndpoint(port: 5004, name: "aggregator-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

builder.AddProject<Projects.StudentPortal_ApiGateway>("gateway")
    .WithReference(enrollmentService)
    .WithReference(coursecatalogService)
    .WithReference(discussionService)
    .WithReference(aggregatorService)
    .WaitFor(enrollmentService)
    .WaitFor(coursecatalogService)
    .WaitFor(discussionService)
    .WaitFor(aggregatorService)
    .WithHttpEndpoint(port: 5000, name: "gateway-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithHttpHealthCheck("/health");

await builder.Build().RunAsync();