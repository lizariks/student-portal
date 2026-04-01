using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles,
                o => o.MapFrom(s =>
                    s.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<UserCreateDto, User>(MemberList.Destination)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore())
            .ForMember(d => d.CoursesAsInstructor, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForSourceMember(s => s.Password, o => o.DoNotValidate());
        
        CreateMap<UserUpdateDto, User>(MemberList.Source)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore())
            .ForMember(d => d.CoursesAsInstructor, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}