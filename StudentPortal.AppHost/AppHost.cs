
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d").WithPgAdmin();

var mongo = builder.AddMongoDB("mongodb").
    WithDataVolume();
var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithDataVolume();
var enrollmentDb = postgres.AddDatabase("EnrollmentDb");
var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");
var discussionDb = mongo.AddDatabase("studentportal-discussion-service-db");
var identityDb = postgres.AddDatabase("studentportal-identityservice-db");
var identityServerDb=postgres.AddDatabase("studentportal-identityserver-db");


var jwtKey = "A_VERY_LONG_AND_SECURE_SECRET_KEY_FOR_JWT_THAT_IS_AT_LEAST_32_CHARACTERS_LONG";
var jwtIssuer = "StudentPortal.IdentityService";
var jwtAudience = "StudentPortal.Services";

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
    //.WithReference(keycloak)
    .WaitFor(discussionDb)
    //.WaitFor(keycloak)
    .WithHttpEndpoint(port: 5003, name: "discussions-http")
    .WithHttpsEndpoint(port: 7049, name: "discussions-https")
     .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
     .WithEnvironment("JwtSettings__Key", jwtKey)
     .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
     .WithEnvironment("JwtSettings__Audience", jwtAudience)
    .WithHttpHealthCheck("/health"); ;

  var enrollmentService=builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollmentservice-api")
    .WithReference(enrollmentDb)
    .WithReference(rabbitmq)
    //.WithReference(keycloak)
    .WaitFor(enrollmentDb)
    .WaitFor(rabbitmq)
    //.WaitFor(keycloak)
    .WithHttpEndpoint(port: 5001, name: "enrollments-http")
    .WithHttpsEndpoint(port: 7047, name: "enrollment-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("JwtSettings__Key", jwtKey)
    .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
    .WithEnvironment("JwtSettings__Audience", jwtAudience);

  var coursecatalogService = builder
      .AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
      .WithReference(catalogDb)
      .WithReference(redis)
      .WithReference(rabbitmq)
     // .WithReference(keycloak)
      .WaitFor(catalogDb)
      .WaitFor(redis)
      .WaitFor(rabbitmq)
      //.WaitFor(keycloak)
      .WithHttpEndpoint(port: 5002, name: "coursescatalog-http")
      .WithHttpsEndpoint(port: 7048, name: "coursecatalog-https")
      .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName).DisableForwardedHeaders()
      .WithEnvironment("JwtSettings__Key", jwtKey)
      .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
      .WithEnvironment("JwtSettings__Audience", jwtAudience);

  var identityService = builder.AddProject<Projects.StudentPortal_IdentityService_Api>("identityservice-api")
      .WithReference(identityDb)
      .WaitFor(identityDb)
      .WithHttpEndpoint(port: 5005, name: "identity-http")
      .WithHttpsEndpoint(port: 7051, name: "identity-https")
      .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
      .WithEnvironment("JwtSettings__Key", jwtKey)
      .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
      .WithEnvironment("JwtSettings__Audience", jwtAudience)
      .WithHttpHealthCheck("/health");

var identityServer = builder.AddProject<Projects.StudentPortal_IdentityServer_Api>("identityserver-api")
.WithReference(identityServerDb)
.WaitFor(identityServerDb)
.WithHttpEndpoint(port: 5010, name: "identityserver-http")
.WithHttpsEndpoint(port: 7052, name: "identityserver-https")
 .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

var aggregatorService = builder.AddProject<Projects.StudentPortal_AggregatorService>("aggregatorservice-api")
    .WithReference(enrollmentService)
    .WithReference(coursecatalogService)
    .WithReference(discussionService)
    //.WithReference(keycloak)
    .WaitFor(enrollmentService)
    .WaitFor(coursecatalogService)
    .WaitFor(discussionService)
    //.WaitFor(keycloak)
    .WithHttpEndpoint(port: 5004, name: "aggregator-http")
    .WithHttpsEndpoint(port: 7050, name: "aggregator-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

builder.AddProject<Projects.StudentPortal_ApiGateway>("gateway")
    .WithReference(enrollmentService)
    .WithReference(coursecatalogService)
    .WithReference(discussionService)
    .WithReference(aggregatorService)
    .WithReference(identityService)
   // .WithReference(keycloak)
    .WaitFor(enrollmentService)
    .WaitFor(coursecatalogService)
    .WaitFor(discussionService)
    .WaitFor(aggregatorService)
    .WaitFor(identityService)
    //.WaitFor(keycloak)
    .WithHttpEndpoint(port: 5000, name: "gateway-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("JwtSettings__Key", jwtKey)
    .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
    .WithEnvironment("JwtSettings__Audience", jwtAudience)
    .WithHttpHealthCheck("/health");

await builder.Build().RunAsync();