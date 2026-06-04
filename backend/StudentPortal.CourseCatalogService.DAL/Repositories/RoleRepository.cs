namespace StudentPortal.CourseCatalogService.DAL.Repositories;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StudentPortal.CourseCatalogService.DAL.Interfaces;
    using StudentPortal.CourseCatalogService.Domain.Entities;
    using StudentPortal.CourseCatalogService.DAL.Data;
    using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
    using StudentPortal.CourseCatalogService.DAL.Helpers;
    using StudentPortal.CourseCatalogService.DAL.Specifications;
    using StudentPortal.CourseCatalogService.DAL.Extensions;

    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly CourseCatalogDbContext _context;

        public RoleRepository(CourseCatalogDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PagedList<Role>> GetPagedRolesAsync(RoleParameters parameters, CancellationToken cancellationToken = default)
        {
            var spec = new RolesWithFiltersSpecification(parameters);
            var query = ApplySpecification(spec);

            return await query.ToPagedListAsync(parameters, cancellationToken);
        }
        
        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Name == name);
        }
       
    }