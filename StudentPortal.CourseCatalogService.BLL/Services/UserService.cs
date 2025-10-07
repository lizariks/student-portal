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


    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IStudentCourseRepository _studentCourseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IStudentCourseRepository studentCourseRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _studentCourseRepository = studentCourseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new BusinessException("Email cannot be empty.");

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> UpdateUserAsync(int userId, User updatedUser, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            user.FirstName = updatedUser.FirstName ?? user.FirstName;
            user.LastName = updatedUser.LastName ?? user.LastName;
            user.Nickname = updatedUser.Nickname ?? user.Nickname;
            user.Email = updatedUser.Email ?? user.Email;
            user.PasswordHash = updatedUser.PasswordHash ?? user.PasswordHash;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            await _userRepository.DeleteAsync(userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new NotFoundException($"Role with id {roleId} not found.");

            if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
            {
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, AssignedAt = DateTime.UtcNow });
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            var roleAssignment = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (roleAssignment != null)
            {
                user.UserRoles.Remove(roleAssignment);
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            return user.UserRoles.Any(ur => ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with id {userId} not found.");

            return user.UserRoles;
        }

        public async Task<IEnumerable<StudentCourse>> GetUserEnrollmentsAsync(int userId)
        {
            return await _studentCourseRepository.GetEnrollmentsByUserAsync(userId);
        }
    }