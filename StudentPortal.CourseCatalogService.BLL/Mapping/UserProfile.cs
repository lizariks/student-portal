namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;


    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, 
                    opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));

            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, UserListDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)))
                .ForMember(dest => dest.CoursesTaughtCount,
                    opt => opt.MapFrom(src => src.CoursesAsInstructor.Count))
                .ForMember(dest => dest.CoursesEnrolledCount,
                    opt => opt.MapFrom(src => src.Enrollments.Count));
        }
    }
