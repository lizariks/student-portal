namespace StudentPortal.CourseCatalogService.BLL.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;
using StudentPortal.CourseCatalogService.BLL.Exceptions;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleDto> CreateRoleAsync(RoleCreateDto roleCreateDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleCreateDto.Name))
            throw new BusinessException("Role name cannot be empty.");

        if (await RoleExistsAsync(roleCreateDto.Name))
            throw new BusinessException($"Role '{roleCreateDto.Name}' already exists.");

        var role = _mapper.Map<Role>(roleCreateDto);

        await _unitOfWork.Roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(int roleId, RoleUpdateDto roleUpdateDto, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
            throw new NotFoundException($"Role with id {roleId} not found.");

        _mapper.Map(roleUpdateDto, role);

        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
            throw new NotFoundException($"Role with id {roleId} not found.");

        await _unitOfWork.Roles.DeleteAsync(roleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        return role == null ? null : _mapper.Map<RoleDto>(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<PagedList<RoleDto>> GetPagedRolesAsync(
        RoleParameters parameters,
        ISortHelper<Role>? sortHelper = null,
        CancellationToken cancellationToken = default)
    {
        var pagedRoles = await _unitOfWork.Roles.GetPagedRolesAsync(parameters,  cancellationToken);
        var mappedItems = _mapper.Map<IEnumerable<RoleDto>>(pagedRoles);

        return new PagedList<RoleDto>(
            mappedItems.ToList(),
            pagedRoles.TotalCount,
            pagedRoles.Page,
            pagedRoles.PageSize);
    }

    public async Task<IEnumerable<RoleDto>> GetUsersInRoleAsync(int roleId)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
            throw new NotFoundException($"Role with id {roleId} not found.");

        return _mapper.Map<IEnumerable<RoleDto>>(role.UserRoles);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }
}