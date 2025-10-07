using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;
using Npgsql;
using System.Data;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<CourseCatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var mapperConfig = MapperConfig.RegisterMappings();
var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseCatalogDbContext>();
    await db.Database.MigrateAsync();
    CourseCatalogSeedDb.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();