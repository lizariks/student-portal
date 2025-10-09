namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.BLL.DTOs.Roles;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using StudentPortal.CourseCatalogService.BLL.DTOs.Users;


public class MapperConfig : Profile
     {
         public MapperConfig()
         {
            CreateMap<Course,CourseDto>();
            CreateMap<Lesson,LessonDto>();
            CreateMap<Material,MaterialDto>();
            CreateMap<Module,ModuleDto>();
            CreateMap<Role,RoleDto>();
            CreateMap<StudentCourse,StudentCourseDto>();
            CreateMap<User,UserDto>();
            
    }
}