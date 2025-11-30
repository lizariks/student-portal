
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d").WithPgAdmin();

var mongo = builder.AddMongoDB("mongodb").
    WithDataVolume();


var enrollmentDb = postgres.AddDatabase("EnrollmentDb");
var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");
var discussionDb = mongo.AddDatabase("studentportal-discussion-service-db");


var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisCommander();
var rabbitmq = builder.AddRabbitMQ("rabbitmq",
        userName: builder.AddParameter("username", "liza", secret: true),
        password: builder.AddParameter("password", "okay123", secret: true))
    .WithManagementPlugin()
    .WithDataVolume();

 var discussionService= builder.AddProject<Projects.StudentPortal_DiscussionService_Api>("discussionservice-api")
    .WithReference(discussionDb)
    .WaitFor(discussionDb)
    .WithHttpEndpoint(port: 5003, name: "discussions-http")
    .WithHttpsEndpoint(port: 7049, name: "discussions-https")
     .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithHttpHealthCheck("/health"); ;

  var enrollmentService=builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollmentservice-api")
    .WithReference(enrollmentDb)
    .WithReference(rabbitmq)
    .WaitFor(enrollmentDb)
    .WaitFor(rabbitmq)
    .WithHttpEndpoint(port: 5001, name: "enrollments-http")
    .WithHttpsEndpoint(port: 7047, name: "enrollment-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);



  var coursecatalogService = builder
      .AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
      .WithReference(catalogDb)
      .WithReference(redis)
      .WithReference(rabbitmq)
      .WaitFor(catalogDb)
      .WaitFor(redis)
      .WaitFor(rabbitmq)
      .WithHttpEndpoint(port: 5002, name: "coursescatalog-http")
      .WithHttpsEndpoint(port: 7048, name: "coursecatalog-https")
      .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName).DisableForwardedHeaders();

var aggregatorService = builder.AddProject<Projects.StudentPortal_AggregatorService>("aggregatorservice-api")
    .WithReference(enrollmentService)
    .WithReference(coursecatalogService)
    .WithReference(discussionService)
    .WaitFor(enrollmentService)
    .WaitFor(coursecatalogService)
    .WaitFor(discussionService)
    .WithHttpEndpoint(port: 5004, name: "aggregator-http")
    .WithHttpsEndpoint(port: 7050, name: "aggregator-https")
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