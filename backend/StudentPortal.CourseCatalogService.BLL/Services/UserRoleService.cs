using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.BLL.DTOs.UserRoles;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW; 
using StudentPortal.CourseCatalogService.Domain.Entities; 
using StudentPortal.ServiceDefaults.Hybrid;
using StudentPortal.Shared.Events.UserRoles; 


namespace StudentPortal.CourseCatalogService.BLL.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserRoleService> _logger;
        private readonly IEntityCacheInvalidationService<UserRole> _cacheInvalidationService; 
        private readonly IHybridCacheService _cacheService;
        private const string CachePrefix = "userrole"; 

        public UserRoleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPublishEndpoint publishEndpoint,
            ILogger<UserRoleService> logger,
            IEntityCacheInvalidationService<UserRole> cacheInvalidationService, 
            IHybridCacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _cacheInvalidationService = cacheInvalidationService;
            _cacheService = cacheService;
        }

        

        public async Task<UserRoleDto?> GetByCompositeKeyAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{CachePrefix}:{userId}:{roleId}";

            return await _cacheService.GetOrSetAsync<UserRoleDto>(
                cacheKey,
                async () =>
                {
                    var ur = await _unitOfWork.UserRoles
                        .GetByCompositeKeyWithReferencesAsync(userId, roleId, cancellationToken);
                    return ur is null ? null : _mapper.Map<UserRoleDto>(ur);
                },
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(30)
            );
        }


        public async Task AssignRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var existingUserRole = await _unitOfWork.UserRoles
                .GetByCompositeKeyWithReferencesAsync(userId, roleId, cancellationToken);
            
            if (existingUserRole != null)
            {
                _logger.LogWarning("Role {RoleId} is already assigned to user {UserId}.", roleId, userId);
                return;
            }
            
            var userRole = new UserRole { UserId = userId, RoleId = roleId };

            await _unitOfWork.UserRoles.AddAsync(userRole, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            var createdUserRole = await _unitOfWork.UserRoles
                .GetByCompositeKeyWithReferencesAsync(userId, roleId, cancellationToken)
                ?? throw new InvalidOperationException($"Failed to load UserRole after creation (User={userId}, Role={roleId}).");

            var createdDto = _mapper.Map<UserRoleDto>(createdUserRole);

            await _cacheInvalidationService.InvalidateAllAsync(); 
            
            await _cacheService.SetAsync($"{CachePrefix}:{userId}:{roleId}", createdDto,
                memoryExpiration: TimeSpan.FromMinutes(2),
                redisExpiration: TimeSpan.FromMinutes(30));

            var @event = new UserRoleCreatedEvent
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = createdUserRole.AssignedAt
            };

            await _publishEndpoint.Publish(@event, cancellationToken);
            _logger.LogInformation("Published UserRoleCreatedEvent for UserId={UserId}, RoleId={RoleId}",
                userId, roleId);
        }

        public async Task UnassignRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var userRoleToDelete = await GetUserRoleOrThrowAsync(userId, roleId, cancellationToken);

            _unitOfWork.UserRoles.Delete(userRoleToDelete); 
            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            await _cacheInvalidationService.InvalidateByIdAsync(userId); 

            var @event = new UserRoleDeletedEvent
            {
                UserId = userId,
                RoleId = roleId,
                DeletedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(@event, cancellationToken);
            _logger.LogInformation("Published UserRoleDeletedEvent for UserId={UserId}, RoleId={RoleId}",
                userId, roleId);
        }

        private async Task<UserRole> GetUserRoleOrThrowAsync(int userId, int roleId, CancellationToken cancellationToken)
        {
            var ur = await _unitOfWork.UserRoles.GetByCompositeKeyWithReferencesAsync(userId, roleId, cancellationToken);
            if (ur == null)
                throw new KeyNotFoundException($"UserRole association (User={userId}, Role={roleId}) not found.");
            return ur;
        }
        
    }
}