namespace StudentPortal.UnitTests.Helpers;

using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;

public static class DbContextFactory
{
    public static CourseCatalogDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CourseCatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
            .Options;
        
        var context = new CourseCatalogDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}