using System.Data;
using Npgsql;
using Serilog;
using AutoMapper;
using StudentPortal.Enrollment.DAL.Interfaces;
using StudentPortal.Enrollment.DAL.Repositories;
using StudentPortal.Enrollment.DAL.UoW;
using StudentPortal.Enrollment.DAL.Connection;
using StudentPortal.Enrollment.BLL.Interfaces;
using StudentPortal.Enrollment.BLL.Services;
using StudentPortal.Enrollment.BLL.Mapping;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.Enrollment.GrpcService.Services;
using StudentPortal.Enrollment.GrpcService.Mapping;
using StudentPortal.ServiceDefaults.Health;




var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddAutoMapperWithLogging(
    typeof(EnrollmentProfile).Assembly,
    typeof(EnrollmentGrpcProfile).Assembly
);

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; 
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});
builder.Services.AddGrpc();

builder.Services.AddScoped<StudentPortal.Enrollment.GrpcService.Services.EnrollmentGrpcServiceImpl>();



builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, o =>
    {
        o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});
// connection string first
var connectionString = builder.Configuration.GetConnectionString("EnrollmentDB")
                       ?? Environment.GetEnvironmentVariable("ENROLLMENT_DB");

// register connection factory
builder.Services.AddScoped<IConnectionFactory>(sp =>
    new ConnectionFactory(connectionString));

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    return connection;
});

// register UoW
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// register repositories (your UoW also initializes them, but if you want DI directly)
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentStatusHistoryRepository, EnrollmentStatusHistoryRepository>();




// register services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentStatusHistoryService, EnrollmentStatusHistoryService>();

// add controllers & swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHealthChecks()
    .AddPostgresHealthCheck(
        configuration: builder.Configuration,
        connectionName: "EnrollmentDB",
        serviceName: "enrollmentservice"
    );

// serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/enrollments-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext());

var app = builder.Build();

// swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGrpcService<EnrollmentGrpcServiceImpl>();
app.MapGrpcServicesWithReflection();

app.Run();
