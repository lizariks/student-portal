using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>();

        CreateMap<RoleCreateDto, Role>(MemberList.Source)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore());

        CreateMap<RoleUpdateDto, Role>(MemberList.Source)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}