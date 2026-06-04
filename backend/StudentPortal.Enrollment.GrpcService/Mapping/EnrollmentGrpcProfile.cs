namespace StudentPortal.Enrollment.GrpcService.Mapping;

using AutoMapper;
using StudentPortal.Enrollment.BLL.DTOs;
using StudentPortal.EnrollmentService.Grpc;
using StudentPortal.EnrollmentService.Grpc;

public class EnrollmentGrpcProfile : Profile
{
    public EnrollmentGrpcProfile()
    {
        CreateMap<EnrollmentDto, StudentPortal.EnrollmentService.Grpc.Enrollment>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.MapFrom(src => src.EnrollmentId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src =>
                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(src.EnrolledAt.ToUniversalTime())));

        CreateMap<StudentPortal.Enrollment.Domain.Entities.Enrollment, StudentPortal.EnrollmentService.Grpc.Enrollment>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.MapFrom(src => src.EnrollmentId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src =>
                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(src.EnrolledAt.ToUniversalTime())));

        CreateMap<EnrollStudentRequest, EnrollmentDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore());

        CreateMap<StudentPortal.EnrollmentService.Grpc.Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.MapFrom(src => src.EnrollmentId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src => src.EnrolledAt.ToDateTime()));
    }
}