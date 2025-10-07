namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;


    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseDto>();

            CreateMap<Course, CourseDetailsDto>()
                .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Modules))
                .ForMember(dest => dest.Enrollments, opt => opt.MapFrom(src => src.Enrollments));

            CreateMap<CourseCreateDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CourseUpdateDto, Course>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .AfterMap((src, dest) =>
                {
                    var type = typeof(Course);
                    foreach (var prop in type.GetProperties())
                    {
                        var value = prop.GetValue(src);
                        if (value != null)
                            prop.SetValue(dest, value);
                    }
                });

            CreateMap<Course, CourseListDto>()
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Instructor.FirstName + " " + src.Instructor.LastName))
                .ForMember(dest => dest.ModulesCount,
                    opt => opt.MapFrom(src => src.Modules.Count))
                .ForMember(dest => dest.EnrollmentsCount,
                    opt => opt.MapFrom(src => src.Enrollments.Count));
        }
    }
