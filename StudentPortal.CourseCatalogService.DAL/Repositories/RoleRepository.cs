namespace StudentPortal.CourseCatalogService.DAL.Repositories;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StudentPortal.CourseCatalogService.DAL.Interfaces;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.CourseCatalogService.DAL.Data;

    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly CourseCatalogDbContext _context;

        public RoleRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Name == name);
        }
       
    }