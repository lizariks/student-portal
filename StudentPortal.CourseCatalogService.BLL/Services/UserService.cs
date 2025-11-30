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
using Microsoft.Extensions.Logging;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.Shared.Events.Users;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.BLL.Metrics;
using StudentPortal.ServiceDefaults.Metrics; 
using MassTransit; 
using Microsoft.Extensions.Logging;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IPublishEndpoint _publishEndpoint; 
    private readonly IEntityCacheInvalidationService<User> _userCacheInvalidationService; 

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper, 
        ILogger<UserService> logger,
        IPublishEndpoint publishEndpoint, 
        IEntityCacheInvalidationService<User> userCacheInvalidationService) 
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _userCacheInvalidationService = userCacheInvalidationService;
    }


    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken cancellationToken = default)
    {
        return await MetricRecorder.RecordOperationAsync(
            CourseMetrics.OperationLatency,
            MetricConstants.Values.Create,
            async () =>
            {
                if (string.IsNullOrWhiteSpace(userCreateDto.Email))
                    throw new BusinessException("Email cannot be empty.");

                var user = _mapper.Map<User>(userCreateDto);
                
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var @event = new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Nickname = user.Nickname,
                    CreatedAt = DateTime.UtcNow
                };
                await _publishEndpoint.Publish(@event, cancellationToken);
                _logger.LogInformation("Published UserCreatedEvent for User {UserId}", user.Id);
                await _userCacheInvalidationService.InvalidateAllAsync();
                
                CourseMetrics.CoursesCreated.Add(1, new System.Diagnostics.TagList(MetricConstants.Tags.OperationCreate) { { "entity", "User" } });

                return _mapper.Map<UserDto>(user);
            });
    }


    public async Task<UserDto> UpdateUserAsync(int userId, UserUpdateDto userUpdateDto, CancellationToken cancellationToken = default)
    {
        return await MetricRecorder.RecordOperationAsync(
            CourseMetrics.OperationLatency,
            MetricConstants.Values.Update,
            async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId, asNoTracking: false, cancellationToken);
                if (user == null)
                    throw new NotFoundException($"User with id {userId} not found.");

                var oldNickname = user.Nickname;
                var oldFirstName = user.FirstName;
                var oldLastName = user.LastName;

                _mapper.Map(userUpdateDto, user);
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (oldNickname != user.Nickname || oldFirstName != user.FirstName || oldLastName != user.LastName)
                {
                    var @event = new UserUpdatedEvent
                    {
                        UserId = user.Id,
                        NewNickname = user.Nickname,
                        NewFirstName = user.FirstName,
                        NewLastName = user.LastName,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _publishEndpoint.Publish(@event, cancellationToken);
                    _logger.LogWarning("Published UserUpdatedEvent for User {UserId}", user.Id);
                }

                await _userCacheInvalidationService.InvalidateByIdAsync(userId);
                
                CourseMetrics.CoursesUpdated.Add(1, new System.Diagnostics.TagList(MetricConstants.Tags.OperationUpdate) { { "entity", "User" } });

                return _mapper.Map<UserDto>(user);
            });
    }


    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await MetricRecorder.RecordOperationAsync(
            CourseMetrics.OperationLatency,
            MetricConstants.Values.Delete,
            async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId, asNoTracking: false, cancellationToken);
                if (user == null)
                    throw new NotFoundException($"User with id {userId} not found.");

                var @event = new UserDeletedEvent
                {
                    UserId = userId,
                    DeletedAt = DateTime.UtcNow
                };
                await _publishEndpoint.Publish(@event, cancellationToken);
                _logger.LogCritical("Published UserDeletedEvent for User {UserId}", userId);
                
                await _unitOfWork.Users.DeleteAsync(userId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _userCacheInvalidationService.InvalidateByIdAsync(userId);
                await _userCacheInvalidationService.InvalidateAllAsync();
                
                CourseMetrics.CoursesDeleted.Add(1, new System.Diagnostics.TagList(MetricConstants.Tags.OperationDelete) { { "entity", "User" } });
            });
    }

    
    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        return await MetricRecorder.RecordOperationAsync(CourseMetrics.OperationLatency, MetricConstants.Values.GetById, 
            async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                return user == null ? null : _mapper.Map<UserDto>(user);
            });
    }

    
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await MetricRecorder.RecordOperationAsync(CourseMetrics.OperationLatency, MetricConstants.Values.List, 
            async () =>
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                return _mapper.Map<IEnumerable<UserDto>>(users);
            });
    }

    public async Task<PagedList<UserDto>> GetPagedUsersAsync(UserParameters parameters, CancellationToken cancellationToken = default)
    {
        return await MetricRecorder.RecordOperationAsync(CourseMetrics.OperationLatency, MetricConstants.Values.List, 
            async () =>
            {
                var pagedUsers = await _unitOfWork.Users.GetPagedUsersAsync(parameters, cancellationToken);
                var mappedItems = _mapper.Map<IEnumerable<UserDto>>(pagedUsers);

                return new PagedList<UserDto>(
                    mappedItems.ToList(),
                    pagedUsers.TotalCount,
                    pagedUsers.Page,
                    pagedUsers.PageSize);
            });
    }
    
    public async Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        await MetricRecorder.RecordOperationAsync(CourseMetrics.OperationLatency, "assign_role", async () =>
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, asNoTracking: false, cancellationToken);
            if (user == null) throw new NotFoundException($"User with id {userId} not found.");

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null) throw new NotFoundException($"Role with id {roleId} not found.");

            if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
            {
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, AssignedAt = DateTime.UtcNow });
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                await _userCacheInvalidationService.InvalidateByIdAsync(userId); 
            }
        });
    }

    public async Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        await MetricRecorder.RecordOperationAsync(CourseMetrics.OperationLatency, "remove_role", async () =>
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, asNoTracking: false, cancellationToken);
            if (user == null) throw new NotFoundException($"User with id {userId} not found.");

            var roleAssignment = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (roleAssignment != null)
            {
                user.UserRoles.Remove(roleAssignment);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _userCacheInvalidationService.InvalidateByIdAsync(userId);
            }
        });
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