using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;

namespace StudentPortal.CourseCatalogService.BLL.Mapping
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseDto>()
                .ForMember(d => d.InstructorName,
                    o => o.MapFrom(s => s.Instructor != null 
                        ? s.Instructor.Nickname 
                        : null));

            CreateMap<Course, CourseDetailsDto>()
                .ForMember(d => d.InstructorName,
                    o => o.MapFrom(s => s.Instructor != null 
                        ? s.Instructor.Nickname 
                        : null))
                .ForMember(d => d.Modules,
                    o => o.MapFrom(s => s.Modules))
                .ForMember(d => d.Enrollments,
                    o => o.MapFrom(s => s.Enrollments));

            CreateMap<CourseCreateDto, Course>(MemberList.Source)
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PublishedAt, o => o.Ignore())
                .ForMember(d => d.Modules, o => o.Ignore())
                .ForMember(d => d.Enrollments, o => o.Ignore())
                .ForMember(d => d.Instructor, o => o.Ignore())
                .ForMember(d => d.CreatedAt,
                    o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt,
                    o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CourseUpdateDto, Course>(MemberList.Source)
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PublishedAt, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.Modules, o => o.Ignore())
                .ForMember(d => d.Enrollments, o => o.Ignore())
                .ForMember(d => d.Instructor, o => o.Ignore())
                .ForMember(d => d.UpdatedAt,
                    o => o.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}