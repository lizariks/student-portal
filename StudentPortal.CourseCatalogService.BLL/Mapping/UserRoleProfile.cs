namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.UserRoles;



public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        CreateMap<UserRole, UserRoleDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Nickname))

            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            
    }
}