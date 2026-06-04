using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;

public class MaterialProfile : Profile
{
    public MaterialProfile()
    {
        CreateMap<Material, MaterialDto>()
            .ForMember(d => d.Type,
                o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Material, MaterialCreateDto>()
            .ForMember(d => d.Type,
                o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Material, MaterialUpdateDto>()
            .ForMember(d => d.Type,
                o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<MaterialCreateDto, Material>(MemberList.Destination)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Lesson, o => o.Ignore())
            .ForMember(d => d.Type,
                o => o.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.Type)
                        ? MaterialType.Video
                        : Enum.Parse<MaterialType>(s.Type, true)));

        CreateMap<MaterialUpdateDto, Material>(MemberList.Destination)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Lesson, o => o.Ignore())
            .ForMember(d => d.LessonId, o => o.Ignore())
            .ForMember(d => d.Type,
                o => o.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.Type)
                        ? MaterialType.Video
                        : Enum.Parse<MaterialType>(s.Type, true)));
    }
}