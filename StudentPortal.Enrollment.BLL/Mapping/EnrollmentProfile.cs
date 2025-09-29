namespace StudentPortal.Enrollment.BLL.Mapping;
using AutoMapper;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.BLL.DTOs;
public class EnrollmentProfile:Profile
{
    public EnrollmentProfile()
    {
        CreateMap<Enrollment, EnrollmentDto>().ReverseMap();
        CreateMap<EnrollmentCreateDto, Enrollment>()
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.StatusHistories, opt => opt.Ignore());
        
    }
}