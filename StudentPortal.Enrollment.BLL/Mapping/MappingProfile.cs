namespace StudentPortal.Enrollment.BLL.Mapping;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Reflection;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.BLL.DTOs;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
           CreateMap<Course, CourseDto>();
           CreateMap<Student, StudentDto>();
           CreateMap<Enrollment, EnrollmentDto>();
           CreateMap<EnrollmentStatusHistory, EnrollmentStatusHistoryDto>();
        }
    }