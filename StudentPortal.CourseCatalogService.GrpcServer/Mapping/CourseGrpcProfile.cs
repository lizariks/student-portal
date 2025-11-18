namespace StudentPortal.CourseCatalogService.GrpcServer.Mapping;


using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using StudentPortal.CourseCatalog.Grpc;
using StudentPortal.CourseCatalogService.BLL.DTOs.Courses; 

public class CourseCatalogGrpcProfile : Profile
{
    public CourseCatalogGrpcProfile()
    {
        CreateMap<CourseDetailsDto, Course>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorId ?? 0))
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => 
                src.PublishedAt.HasValue 
                    ? Timestamp.FromDateTime(src.PublishedAt.Value.ToUniversalTime()) 
                    : null))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => 
                Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => 
                Timestamp.FromDateTime(src.UpdatedAt.ToUniversalTime())));
        CreateMap<CourseDto, Course>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty)) // Description може бути null
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorId ?? 0)) // Nullable int
            
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => 
                src.PublishedAt.HasValue 
                    ? Timestamp.FromDateTime(src.PublishedAt.Value.ToUniversalTime()) 
                    : null));


        CreateMap<Course, CourseUpdateDto>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorId));
    }
}