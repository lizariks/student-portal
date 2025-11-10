namespace StudentPortal.DiscussionService.GrpcServer.Mapping;

using AutoMapper;
using Google.Protobuf.WellKnownTypes; 
using StudentPortal.Discussion.Grpc; 

using DomainThread = StudentPortal.DiscussionService.Domain.Entities.DiscussionThread;
using DomainComment = StudentPortal.DiscussionService.Domain.Entities.Comment; 
using DomainUserInfo = StudentPortal.DiscussionService.Domain.ValueObjects.UserInfo;
using DomainTargetType = StudentPortal.DiscussionService.Domain.Enums.TargetType;
using DomainUserRole = StudentPortal.DiscussionService.Domain.ValueObjects.UserRole;

public class DiscussionGrpcProfile : Profile
{
    public DiscussionGrpcProfile()
    {
        CreateMap<DomainUserRole, string>()
            .ConvertUsing(src => src.ToString()); 

        CreateMap<DomainUserInfo, UserInfo>() 
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString())); 
       
        CreateMap<DomainTargetType, TargetType>()
            .ConvertUsing(src => (TargetType)(int)src);

        CreateMap<DomainComment, Comment>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content)) 
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.UpdatedAt.ToUniversalTime())));

        CreateMap<DomainThread, DiscussionThread>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.UpdatedAt.ToUniversalTime())));
        CreateMap<string, DomainUserRole>();

        CreateMap<UserInfo, DomainUserInfo>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));
        
        CreateMap<TargetType, DomainTargetType>()
            .ConvertUsing(src => (DomainTargetType)(int)src);

        CreateMap<StudentPortal.Discussion.Grpc.AddCommentRequest, DomainComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author)) 
            
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content)) 
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}