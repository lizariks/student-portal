namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;


    public class StudentCourseProfile : Profile
    {
        public StudentCourseProfile()
        {
            CreateMap<StudentCourse, StudentCourseDto>();
            CreateMap<StudentCourseCreateDto, StudentCourse>()
                .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
