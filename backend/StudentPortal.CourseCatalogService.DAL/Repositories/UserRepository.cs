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

    public class UserRepository : GenericRepository<User>,IUserRepository
    {
        private readonly CourseCatalogDbContext _context;

        public UserRepository(CourseCatalogDbContext context): base(context)
        {
            _context = context;
        }
        public async Task<PagedList<User>> GetPagedUsersAsync(UserParameters parameters, CancellationToken cancellationToken = default)
        {
            var spec = new UsersWithFiltersSpecification(parameters);
            var query = ApplySpecification(spec);

            var count = await query.CountAsync(cancellationToken);
            var items = await query.ToListAsync(cancellationToken);

            return new PagedList<User>(items, count, parameters.Page, parameters.PageSize);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetInstructorsAsync()
        {
            return await _context.Users
                .Where(u => u.CoursesAsInstructor.Any())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.Enrollments.Any())
                .AsNoTracking()
                .ToListAsync();
        }
    }
