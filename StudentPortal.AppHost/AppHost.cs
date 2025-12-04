using StudentPortal.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("postgres-username", "postgres", secret: true);
var postgresPass = builder.AddParameter("postgres-password", "8289", secret: true);

var mongoUser = builder.AddParameter("mongo-username", "mongoadmin", secret: true);
var mongoPass = builder.AddParameter("mongo-password", "mongo123", secret: true);

var redisPass = builder.AddParameter("redis-password", "redis123", secret: true);

var rabbitUser = builder.AddParameter("rabbitmq-username", "liza", secret: true);
var rabbitPass = builder.AddParameter("rabbitmq-password", "okay123", secret: true);

var keycloakAdminUser = builder.AddParameter("keycloak-admin-username", "admin", secret: true);
var keycloakAdminPass = builder.AddParameter("keycloak-admin-password", "admin", secret: true);

var postgres = builder.AddPostgres("postgres",
        userName: postgresUser,
        password: postgresPass)
    .WithEnvironment("PGSSLMODE", "disable")
    .WithDataVolume()
    .WithBindMount("db", "/docker-entrypoint-initdb.d")
    .WithPgAdmin();

var mongo = builder.AddMongoDB("mongodb",
        userName: mongoUser,
        password: mongoPass)
    .WithDataVolume();

var redis = builder.AddRedis("redis", password: redisPass)
    .WithDataVolume()
    .WithRedisCommander();

var rabbitmq = builder.AddRabbitMQ("rabbitmq",
        userName: rabbitUser,
        password: rabbitPass)
    .WithManagementPlugin()
    .WithDataVolume();

var enrollmentDb = postgres.AddDatabase("studentportal-enrollment-db");
var catalogDb = postgres.AddDatabase("studentportal-catalogcourses-db");
var discussionDb = mongo.AddDatabase("studentportal-discussion-service-db");

// var identityDb = postgres.AddDatabase("studentportal-identityservice-db");
// var identityServerDb = postgres.AddDatabase("studentportal-identityserver-db");

var keycloak = builder.AddKeycloak("keycloak", port: 8080, keycloakAdminUser, keycloakAdminPass)
    .WithDataVolume()
    .WithAutoConfiguration();

var keycloakUrl = keycloak.GetEndpoint("http");
var keycloakRealm = "StudentPortal";
var keycloakAudience = "studentportal_api";


var courseCatalogService = builder.AddProject<Projects.StudentPortal_CourseCatalogService_Apii>("coursecatalogservice-api")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(catalogDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak)
    .WithHttpEndpoint(port: 5002, name: "coursescatalog-http")
    .WithHttpsEndpoint(port: 7048, name: "coursecatalog-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithKeycloakEnvironment(keycloakUrl, keycloakRealm, keycloakAudience)
    .WithHttpHealthCheck("/health");

var discussionService = builder.AddProject<Projects.StudentPortal_DiscussionService_Api>("discussionservice-api")
    .WithReference(discussionDb)
    .WithReference(courseCatalogService)
    .WithReference(keycloak)
    .WaitFor(discussionDb)
    .WaitFor(courseCatalogService)
    .WaitFor(keycloak)
    .WithHttpEndpoint(port: 5003, name: "discussions-http")
    .WithHttpsEndpoint(port: 7049, name: "discussions-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithKeycloakEnvironment(keycloakUrl, keycloakRealm, keycloakAudience)
    .WithHttpHealthCheck("/health");

var enrollmentService = builder.AddProject<Projects.StudentPortal_Enrollment_Api>("enrollmentservice-api")
    .WithReference(enrollmentDb)
    .WithReference(courseCatalogService)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(enrollmentDb)
    .WaitFor(courseCatalogService)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak)
    .WithHttpEndpoint(port: 5001, name: "enrollments-http")
    .WithHttpsEndpoint(port: 7047, name: "enrollment-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithKeycloakEnvironment(keycloakUrl, keycloakRealm, keycloakAudience)
    .WithHttpHealthCheck("/health");

var aggregatorService = builder.AddProject<Projects.StudentPortal_AggregatorService>("aggregatorservice-api")
    .WithReference(enrollmentService)
    .WithReference(courseCatalogService)
    .WithReference(discussionService)
    .WithReference(keycloak)
    .WaitFor(enrollmentService)
    .WaitFor(courseCatalogService)
    .WaitFor(discussionService)
    .WaitFor(keycloak)
    .WithHttpEndpoint(port: 5004, name: "aggregator-http")
    .WithHttpsEndpoint(port: 7050, name: "aggregator-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.StudentPortal_ApiGateway>("gateway")
    .WithReference(enrollmentService)
    .WithReference(courseCatalogService)
    .WithReference(discussionService)
    .WithReference(aggregatorService)
    .WithReference(keycloak)
    .WaitFor(enrollmentService)
    .WaitFor(courseCatalogService)
    .WaitFor(discussionService)
    .WaitFor(aggregatorService)
    .WaitFor(keycloak)
    .WithHttpEndpoint(port: 5000, name: "gateway-http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithKeycloakEnvironment(keycloakUrl, keycloakRealm, keycloakAudience)
    .WithHttpHealthCheck("/health");

await builder.Build().RunAsync();