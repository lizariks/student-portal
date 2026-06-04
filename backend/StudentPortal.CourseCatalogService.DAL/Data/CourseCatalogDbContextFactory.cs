using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StudentPortal.CourseCatalogService.DAL.Data;
using Npgsql;
namespace StudentPortal.CourseCatalogService.DAL.Data
{
    public class CourseCatalogDbContextFactory : IDesignTimeDbContextFactory<CourseCatalogDbContext>
    {
        public CourseCatalogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CourseCatalogDbContext>();
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=studentportal-catalogcourses-db;Username=postgres;Password=8289"
            );

            return new CourseCatalogDbContext(optionsBuilder.Options);
        }
    }
}