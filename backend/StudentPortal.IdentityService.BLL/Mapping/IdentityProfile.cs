namespace StudentPortal.IdentityService.BLL.Mapping;
using AutoMapper;
using StudentPortal.IdentityService.BLL.DTOs;
using StudentPortal.IdentityService.Domain.Entities;
public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<RefreshToken, RefreshTokenDto>();
    }
}