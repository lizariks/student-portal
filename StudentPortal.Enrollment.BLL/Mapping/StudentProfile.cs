using AutoMapper;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.BLL.DTOs;
namespace StudentPortal.Enrollment.BLL.Mapping;

public class StudentProfile:Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>().ReverseMap();
    }
}