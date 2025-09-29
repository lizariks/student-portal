namespace StudentPortal.Enrollment.BLL.Mapping;
using AutoMapper;
using StudentPortal.Enrollment.Domain;
using StudentPortal.Enrollment.BLL.DTOs;
public class EnrollmentStatusHistoryProfile:Profile
{
    public EnrollmentStatusHistoryProfile()
    {
        CreateMap<EnrollmentStatusHistoryProfile, EnrollmentStatusHistoryDto>().ReverseMap();
        CreateMap<EnrollmentStatusHistoryUpdateDto, EnrollmentStatusHistory>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}