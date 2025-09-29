namespace StudentPortal.Enrollment.BLL.Mapping;
using AutoMapper;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.BLL.DTOs;
public class CourseProfile:Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseDto>().ReverseMap();
    }
}