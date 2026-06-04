using StudentPortal.IdentityService.Domain.Entities;
using StudentPortal.IdentityService.DAL.Data;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.ServiceDefaults.Health;
using  StudentPortal.IdentityService.BLL.Config;
using  StudentPortal.IdentityService.BLL.Interfaces;
using  StudentPortal.IdentityService.BLL.Services;
using  StudentPortal.IdentityService.BLL.Mapping;
using StudentPortal.IdentityService.DAL.Repositories;
using StudentPortal.IdentityService.DAL.Interfaces;


using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

var connectionString = builder.Configuration.GetConnectionString("studentportal-identityservice-db")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
                       

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString!));   

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAutoMapperWithLogging(typeof(IdentityProfile).Assembly);

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddHealthChecks()
    .AddPostgresHealthCheck(
        configuration: builder.Configuration,
        connectionName: "studentportal-identityservice-db",
        serviceName: "identityservice",
        timeoutSeconds: 5);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(app.Services);
}


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCorrelationId();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();                    