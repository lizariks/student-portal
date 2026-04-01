using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;

public class StudentCourseProfile : Profile
{
    public StudentCourseProfile()
    {
        CreateMap<StudentCourse, StudentCourseDto>();

        CreateMap<StudentCourseCreateDto, StudentCourse>(MemberList.Source)
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore())
            .ForMember(d => d.EnrolledAt,
                o => o.MapFrom(_ => DateTime.UtcNow));
    }
}