namespace StudentPortal.Enrollment.BLL.Mapping;
using AutoMapper;
using StudentPortal.Enrollment.Domain.Entities;
using StudentPortal.Enrollment.BLL.DTOs;
public class CourseProfile:Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseDto>().ReverseMap();
    }
}