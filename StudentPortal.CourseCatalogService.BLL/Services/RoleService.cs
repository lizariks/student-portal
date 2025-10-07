namespace StudentPortal.CourseCatalogService.BLL.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;


    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Role> CreateRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
                throw new BusinessException("Role name cannot be empty.");

            if (await RoleExistsAsync(role.Name))
                throw new BusinessException($"Role '{role.Name}' already exists.");

            await _roleRepository.AddAsync(role, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return role;
        }

        public async Task<Role> UpdateRoleAsync(int roleId, Role updatedRole, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new NotFoundException($"Role with id {roleId} not found.");

            role.Name = updatedRole.Name ?? role.Name;

            await _roleRepository.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return role;
        }

        public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new NotFoundException($"Role with id {roleId} not found.");

            await _roleRepository.DeleteAsync(roleId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetByIdAsync(roleId);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUsersInRoleAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new NotFoundException($"Role with id {roleId} not found.");

            return role.UserRoles;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
