namespace StudentPortal.CourseCatalogService.BLL.Services;

using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userCreateDto.Email))
            throw new BusinessException("Email cannot be empty.");

        var user = _mapper.Map<User>(userCreateDto);
        
        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UserUpdateDto userUpdateDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found.");

        _mapper.Map(userUpdateDto, user);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Users.DeleteAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserListDto>>(users);
    }

    public async Task<PagedList<UserListDto>> GetPagedUsersAsync(
        UserParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var pagedUsers = await _unitOfWork.Users.GetPagedUsersAsync(parameters, cancellationToken);
        var mappedItems = _mapper.Map<IEnumerable<UserListDto>>(pagedUsers);

        return new PagedList<UserListDto>(
            mappedItems.ToList(),
            pagedUsers.TotalCount,
            pagedUsers.Page,
            pagedUsers.PageSize);
    }

    public async Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found.");

        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
            throw new NotFoundException($"Role with id {roleId} not found.");

        if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, AssignedAt = DateTime.UtcNow });
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found.");

        var roleAssignment = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (roleAssignment != null)
        {
            user.UserRoles.Remove(roleAssignment);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found.");

        return user.UserRoles.Any(ur => ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found.");

        return user.UserRoles.Select(ur => ur.Role.Name);
    }

    public async Task<IEnumerable<StudentCourseDto>> GetUserEnrollmentsAsync(int userId)
    {
        var enrollments = await _unitOfWork.StudentCourses.GetEnrollmentsByUserAsync(userId);
        return _mapper.Map<IEnumerable<StudentCourseDto>>(enrollments);
    }
}