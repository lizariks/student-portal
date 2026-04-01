using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.UserRoles;

public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        CreateMap<UserRole, UserRoleDto>()
            .ForMember(d => d.UserName,
                o => o.MapFrom(s => s.User != null ? s.User.Nickname : null))
            .ForMember(d => d.RoleName,
                o => o.MapFrom(s => s.Role != null ? s.Role.Name : null));
    }
}