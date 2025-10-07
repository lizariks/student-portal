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
using StudentPortal.Enrollment.Domain.Exceptions;

var builder = WebApplication.CreateBuilder(args);

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

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);



// register services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentStatusHistoryService, EnrollmentStatusHistoryService>();

// add controllers & swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
